using PSListMaker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PSListMaker.Searchers
{
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

            // Выделяем найденный элемент
            SelectItem(_currentMatches[_currentIndex]);
        }

        private List<TreeViewItem> GetAllMatchingTreeViewItems(string searchText)
        {
            var allItems = GetAllTreeViewItems(_treeView);
            return allItems
                .Where(item => (item.Header as ComponentMin)?.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
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

            bool wasExpanded = item.IsExpanded;
            if (!item.IsExpanded)
                item.IsExpanded = true;

            item.UpdateLayout();

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
            // Снимаем выделение с предыдущего элемента
            if (_currentMatches != null)
            {
                foreach (var oldItem in _currentMatches)
                {
                    if (oldItem != item)
                        oldItem.IsSelected = false;
                }
            }

            item.IsSelected = true;
            item.BringIntoView();
            item.Focus();
        }

        public void Reset()
        {
            _lastSearchText = null;
            _currentMatches = null;
            _currentIndex = -1;
        }
    }
}