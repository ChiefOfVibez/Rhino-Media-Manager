using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using BoschMediaBrowser.Core.Models;

namespace BoschMediaBrowser.Rhino.UI.Controls
{
    /// <summary>
    /// Hierarchical category navigation tree
    /// Shows Range > Category structure (e.g., PRO > Garden, DIY > Drills)
    /// </summary>
    public class CategoryList : Panel
    {
        private TreeGridView _treeView;
        private List<Product> _allProducts = new();
        
        public event EventHandler<CategorySelectedEventArgs>? CategorySelected;
        
        public CategoryList()
        {
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            _treeView = new TreeGridView
            {
                BackgroundColor = Colors.White,
                ShowHeader = false,
                Border = BorderType.None
            };
            
            // Single column for category names
            _treeView.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell { Binding = Binding.Property<CategoryNode, string>(n => n.DisplayText) },
                HeaderText = "Categories",
                Expand = true
            });
            
            _treeView.SelectionChanged += OnSelectionChanged;
            
            var scrollable = new Scrollable
            {
                Content = _treeView,
                Border = BorderType.None
            };
            
            Content = scrollable;
        }
        
        /// <summary>
        /// Load products and build category list
        /// </summary>
        public void LoadProducts(List<Product> products)
        {
            _allProducts = products ?? new List<Product>();
            BuildCategoryList();
        }
        
        private void BuildCategoryList()
        {
            _treeView.DataStore = null;
            
            if (_allProducts.Count == 0)
            {
                return;
            }
            
            var rootNodes = new List<CategoryNode>();
            
            // Add "All Products" root node
            var allProductsNode = new CategoryNode
            {
                DisplayText = $"All Products ({_allProducts.Count})",
                CategoryType = "All",
                RangeFilter = null,
                CategoryFilter = null,
                Products = _allProducts
            };
            rootNodes.Add(allProductsNode);
            
            // Group by Range (PRO, DIY, etc.), then by Category
            var rangeGroups = _allProducts
                .Where(p => !string.IsNullOrEmpty(p.Range))
                .GroupBy(p => p.Range)
                .OrderBy(g => g.Key)
                .ToList();
            
            foreach (var rangeGroup in rangeGroups)
            {
                var rangeName = rangeGroup.Key;
                var rangeProducts = rangeGroup.ToList();
                
                // Create range node (e.g., "PRO", "DIY")
                var rangeNode = new CategoryNode
                {
                    DisplayText = $"{rangeName} ({rangeProducts.Count})",
                    CategoryType = "Range",
                    RangeFilter = rangeName,
                    CategoryFilter = null,
                    Products = rangeProducts
                };
                
                // Add category children under each range
                var categoryGroups = rangeProducts
                    .GroupBy(p => p.Category ?? "Uncategorized")
                    .OrderBy(g => g.Key)
                    .ToList();
                
                foreach (var categoryGroup in categoryGroups)
                {
                    var categoryName = categoryGroup.Key;
                    var categoryProducts = categoryGroup.ToList();
                    
                    var categoryNode = new CategoryNode
                    {
                        DisplayText = $"{categoryName} ({categoryProducts.Count})",
                        CategoryType = "Category",
                        RangeFilter = rangeName,
                        CategoryFilter = categoryName,
                        Products = categoryProducts
                    };
                    
                    rangeNode.AddChild(categoryNode);
                }
                
                rootNodes.Add(rangeNode);
            }
            
            _treeView.DataStore = new TreeGridItemCollection(rootNodes);
            
            // Expand all and select "All Products" by default
            _treeView.ReloadData();
            if (rootNodes.Count > 0)
            {
                _treeView.SelectedItem = allProductsNode;
            }
            
            global::Rhino.RhinoApp.WriteLine($"CategoryList: Built tree with {rangeGroups.Count} ranges, {_allProducts.Count} total products");
        }
        
        private void OnSelectionChanged(object? sender, EventArgs e)
        {
            var selectedNode = _treeView.SelectedItem as CategoryNode;
            if (selectedNode == null)
                return;
            
            var filteredProducts = selectedNode.Products;
            var displayName = selectedNode.DisplayText;
            
            global::Rhino.RhinoApp.WriteLine($"CategoryList: Selected '{displayName}', {filteredProducts.Count} products");
            
            CategorySelected?.Invoke(this, new CategorySelectedEventArgs(displayName, filteredProducts));
        }
        
        /// <summary>
        /// Get currently selected category
        /// </summary>
        public string GetSelectedCategory()
        {
            var selectedNode = _treeView.SelectedItem as CategoryNode;
            return selectedNode?.DisplayText ?? "All Products";
        }
    }
    
    /// <summary>
    /// Tree node for hierarchical categories
    /// </summary>
    public class CategoryNode : ITreeGridItem<CategoryNode>
    {
        private List<CategoryNode> _children = new();
        
        public string DisplayText { get; set; } = "";
        public string CategoryType { get; set; } = ""; // "All", "Range", "Category"
        public string? RangeFilter { get; set; }
        public string? CategoryFilter { get; set; }
        public List<Product> Products { get; set; } = new();
        
        // Method to add child nodes
        public void AddChild(CategoryNode child)
        {
            _children.Add(child);
            child.Parent = this;
        }
        
        // ITreeGridItem implementation
        public ITreeGridItem Parent { get; set; }
        public IEnumerable<CategoryNode> Children => _children;
        public bool Expanded { get; set; } = false;
        public bool Expandable => _children.Count > 0;
        
        // IDataStore implementation
        public int Count => _children.Count;
        public CategoryNode this[int index] => _children[index];
    }
    
    /// <summary>
    /// Event args for category selection
    /// </summary>
    public class CategorySelectedEventArgs : EventArgs
    {
        public string Category { get; }
        public List<Product> FilteredProducts { get; }
        
        public CategorySelectedEventArgs(string category, List<Product> filteredProducts)
        {
            Category = category;
            FilteredProducts = filteredProducts;
        }
    }
}
