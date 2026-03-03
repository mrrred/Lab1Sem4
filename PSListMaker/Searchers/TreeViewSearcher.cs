using System;
using System.Collections.Generic;
using System.Text;

namespace PSListMaker.Searchers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    public class TreeViewSearcher
    {
        private readonly TreeView _treeView;
        private string _lastSearchText;
        private List<TreeViewItem> _currentMatches;
        private int _currentIndex = -1;

        public TreeViewSearcher(TreeView treeView)
        {
            _treeView = treeView;
        }

        public void FindNext(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
                return;

            if (_lastSearchText != searchText)
            {
                _lastSearchText = searchText;
                _currentMatches = GetAllMatchingTreeViewItems(searchText);
                _currentIndex = -1;
            }

            if (_currentMatches == null || _currentMatches.Count == 0)
            {
                MessageBox.Show("Совпадений не найдено.", "Поиск", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _currentIndex++;
            if (_currentIndex >= _currentMatches.Count)
            {
                MessageBox.Show("Достигнут конец списка. Начинаем сначала.", "Поиск", MessageBoxButton.OK, MessageBoxImage.Information);
                _currentIndex = 0;
            }

            SelectItem(_currentMatches[_currentIndex]);
        }
        private List<TreeViewItem> GetAllMatchingTreeViewItems(string searchText)
        {
            var allItems = GetAllTreeViewItems(_treeView);
            return allItems
                .Where(item => item.Header?.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }
        private List<TreeViewItem> GetAllTreeViewItems(TreeView treeView)
        {
            var result = new List<TreeViewItem>();

            foreach (var item in treeView.Items)
            {
                var container = treeView.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (container != null)
                {
                    TraverseTreeViewItem(container, result);
                }
            }

            return result;
        }

        private void TraverseTreeViewItem(TreeViewItem item, List<TreeViewItem> list)
        {
            list.Add(item);

            if (!item.IsExpanded)
                item.IsExpanded = true;

            foreach (var child in item.Items)
            {
                var childContainer = item.ItemContainerGenerator.ContainerFromItem(child) as TreeViewItem;
                if (childContainer != null)
                {
                    TraverseTreeViewItem(childContainer, list);
                }
            }
        }

        private void SelectItem(TreeViewItem item)
        {
            if (_currentIndex > 0 && _currentIndex - 1 < _currentMatches.Count)
            {
                var prevItem = _currentMatches[_currentIndex - 1];
                prevItem.IsSelected = false;
            }
            else if (_currentIndex == 0 && _currentMatches.Count > 1)
            {
                _currentMatches.Last().IsSelected = false;
            }

            item.IsSelected = true;
            item.BringIntoView();
            item.Focus();
        }
    }
}
