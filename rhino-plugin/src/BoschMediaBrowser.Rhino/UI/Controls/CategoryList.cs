using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using Eto.Drawing;
using BoschMediaBrowser.Core.Models;

namespace BoschMediaBrowser.Rhino.UI.Controls
{
    /// <summary>
    /// Simple category navigation list (replaces TreeGridView to avoid DataStore freeze)
    /// </summary>
    public class CategoryList : Panel
    {
        private ListBox _listBox;
        private List<Product> _allProducts = new();
        private Dictionary<string, int> _categoryCounts = new();
        
        public event EventHandler<CategorySelectedEventArgs>? CategorySelected;
        
        public CategoryList()
        {
            InitializeUI();
        }
        
        private void InitializeUI()
        {
            _listBox = new ListBox
            {
                BackgroundColor = Colors.White
            };
            
            _listBox.SelectedIndexChanged += OnSelectionChanged;
            
            Content = _listBox;
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
            _listBox.Items.Clear();
            _categoryCounts.Clear();
            
            if (_allProducts.Count == 0)
            {
                return;
            }
            
            // Count products per category
            var categoryGroups = _allProducts
                .GroupBy(p => p.Category ?? "Uncategorized")
                .OrderBy(g => g.Key)
                .ToList();
            
            // Add "All Products" option
            _listBox.Items.Add($"All Products ({_allProducts.Count})");
            _categoryCounts["All Products"] = _allProducts.Count;
            
            // Add each category with count
            foreach (var group in categoryGroups)
            {
                var displayText = $"{group.Key} ({group.Count()})";
                _listBox.Items.Add(displayText);
                _categoryCounts[group.Key] = group.Count();
            }
            
            // Select "All Products" by default
            if (_listBox.Items.Count > 0)
            {
                _listBox.SelectedIndex = 0;
            }
            
            global::Rhino.RhinoApp.WriteLine($"CategoryList: Loaded {categoryGroups.Count} categories, {_allProducts.Count} total products");
        }
        
        private void OnSelectionChanged(object? sender, EventArgs e)
        {
            if (_listBox.SelectedIndex < 0)
                return;
            
            var selectedText = _listBox.SelectedValue?.ToString() ?? "";
            
            // Extract category name (remove count)
            var category = ExtractCategoryName(selectedText);
            
            // Filter products
            List<Product> filteredProducts;
            
            if (category == "All Products")
            {
                filteredProducts = _allProducts;
            }
            else
            {
                filteredProducts = _allProducts
                    .Where(p => (p.Category ?? "Uncategorized") == category)
                    .ToList();
            }
            
            global::Rhino.RhinoApp.WriteLine($"CategoryList: Selected '{category}', {filteredProducts.Count} products");
            
            CategorySelected?.Invoke(this, new CategorySelectedEventArgs(category, filteredProducts));
        }
        
        private string ExtractCategoryName(string displayText)
        {
            // Extract "Category Name" from "Category Name (123)"
            var parenIndex = displayText.LastIndexOf('(');
            if (parenIndex > 0)
            {
                return displayText.Substring(0, parenIndex).Trim();
            }
            return displayText.Trim();
        }
        
        /// <summary>
        /// Get currently selected category
        /// </summary>
        public string GetSelectedCategory()
        {
            if (_listBox.SelectedIndex >= 0)
            {
                return ExtractCategoryName(_listBox.SelectedValue?.ToString() ?? "");
            }
            return "All Products";
        }
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
