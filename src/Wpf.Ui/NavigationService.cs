// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using Wpf.Ui.Abstractions;
using Wpf.Ui.Controls;

namespace Wpf.Ui;

/// <summary>
/// A service that provides methods related to navigation.
/// </summary>
public partial class NavigationService(INavigationViewPageProvider pageProvider) : INavigationService
{
    private string _currentPage = string.Empty;

    public string CurrentPage
    {
        get => _currentPage;
        private set
        {
            if (_currentPage != value)
            {
                _currentPage = value;
                CurrentPageChanged?.Invoke(this, _currentPage);
            }
        }
    }

    /// <summary>
    /// Gets or sets the control representing navigation.
    /// </summary>
    protected INavigationView? NavigationControl { get; set; }

    /// <inheritdoc />
    public INavigationView GetNavigationControl()
    {
        return NavigationControl ?? throw new ArgumentNullException(nameof(NavigationControl));
    }

    /// <inheritdoc />
    public void SetNavigationControl(INavigationView navigation)
    {
        NavigationControl = navigation;
        NavigationControl.SetPageProviderService(pageProvider);
    }

    /// <inheritdoc />
    public bool Navigate(Type pageType)
    {
        ThrowIfNavigationControlIsNull();

        return NavigationControl!.Navigate(pageType);
    }

    /// <inheritdoc />
    public bool Navigate(Type pageType, object? dataContext)
    {
        ThrowIfNavigationControlIsNull();

        // 1) 실제 네비게이션 시도
        var result = NavigationControl!.Navigate(pageType, dataContext);

        // 2) 성공했다면 SelectedItem 의 TargetPageTag 를 읽어서 이벤트 발생
        if (result)
        {
            var tag = NavigationControl.SelectedItem?.TargetPageTag;
            if (!string.IsNullOrEmpty(tag))
            {
                UpdateCurrentPage(tag);
            }
        }

        return result;
    }

    /// <inheritdoc />
    public bool Navigate(string pageTag)
    {
        ThrowIfNavigationControlIsNull();

        return NavigationControl!.Navigate(pageTag);
    }

    /// <inheritdoc />
    public bool Navigate(string pageTag, object? dataContext)
    {
        ThrowIfNavigationControlIsNull();

        return NavigationControl!.Navigate(pageTag, dataContext);
    }

    /// <inheritdoc />
    public bool GoBack()
    {
        ThrowIfNavigationControlIsNull();

        return NavigationControl!.GoBack();
    }

    /// <inheritdoc />
    public bool NavigateWithHierarchy(Type pageType)
    {
        ThrowIfNavigationControlIsNull();

        return NavigationControl!.NavigateWithHierarchy(pageType);
    }

    /// <inheritdoc />
    public bool NavigateWithHierarchy(Type pageType, object? dataContext)
    {
        ThrowIfNavigationControlIsNull();

        return NavigationControl!.NavigateWithHierarchy(pageType, dataContext);
    }

    protected void ThrowIfNavigationControlIsNull()
    {
        if (NavigationControl is null)
        {
            throw new ArgumentNullException(nameof(NavigationControl));
        }
    }

    // Added by SHJ - 2025-06-02
    // [문제 배경]
    // NavigationView에 자식 항목(NavigationViewItem)을 동적으로 추가한 후,
    // 해당 페이지로 이동하거나 탐색 기록(Journal)을 활용해 복귀하는 과정에서
    // NavigationView 내부 딕셔너리(PageIdOrTargetTagNavigationViewsDictionary)에
    // 해당 자식 항목의 Id(Tag)가 등록되지 않아 예외가 발생함.
    //
    // [문제 원인]
    // NavigationViewItem은 기본적으로 RegisterNavigationViewItem을 통해
    // NavigationView 내부 탐색용 딕셔너리(PageIdOrTargetTagNavigationViewsDictionary)에 등록되어야 함.
    // 하지만 MenuItems를 통해 동적으로 자식을 추가할 경우 OnMenuItemsSource_CollectionChanged 이벤트가
    // 호출되지 않거나, 내부 등록 로직이 수행되지 않아 해당 키가 누락됨.
    //
    // [해결 방법]
    // NavigationService를 통해 동적으로 추가되는 자식 NavigationViewItem을 수동으로
    // RegisterNavigationViewItem()에 전달하여 강제로 딕셔너리에 등록함으로써,
    // Navigate(tag) 또는 Journal 복원 시 키를 찾지 못해 발생하는 예외를 방지함.
    public void RegisterNavigationViewItem(INavigationViewItem item)
    {
        if (NavigationControl is NavigationView navView)
        {
            navView.RegisterNavigationViewItem(item);
        }
    }

    public event EventHandler<string>? CurrentPageChanged;

    public void UpdateCurrentPage(string newPage)
    {
        CurrentPage = newPage;
    }
}