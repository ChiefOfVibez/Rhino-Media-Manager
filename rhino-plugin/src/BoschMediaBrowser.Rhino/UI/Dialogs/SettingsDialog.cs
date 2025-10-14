using Eto.Forms;
using Eto.Drawing;
using BoschMediaBrowser.Core.Services;
using System;

namespace BoschMediaBrowser.Rhino.UI.Dialogs;

/// <summary>
/// Settings dialog for configuring database path and other preferences
/// </summary>
public class SettingsDialog : Dialog<bool>
{
    private readonly SettingsService _settingsService;
    private TextBox _basePathTextBox;
    private Label _statusLabel;
    
    // Insert options controls
    private CheckBox _readLinkedBlocksCheckBox;
    private RadioButton _blockTypeEmbeddedRadio;
    private RadioButton _blockTypeLinkedRadio;
    private RadioButton _layerStyleActiveRadio;
    private RadioButton _layerStyleReferenceRadio;
    private RadioButton _insertAsBlockRadio;
    private RadioButton _insertAsGroupRadio;

    public SettingsDialog(SettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        
        Title = "Bosch Media Browser - Settings";
        MinimumSize = new Size(600, 550);
        Padding = 20;
        
        BuildUI();
    }

    private void BuildUI()
    {
        // Create status label first
        _statusLabel = new Label
        {
            Text = "Configure the path to your Bosch product database",
            TextColor = Color.FromArgb(100, 100, 100),
            Wrap = WrapMode.Word
        };
        
        var layout = new StackLayout
        {
            Spacing = 15,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Items =
            {
                // Header
                new Label
                {
                    Text = "Settings",
                    Font = new Font(SystemFont.Bold, 16)
                },
                
                // Database Path Section
                new Label
                {
                    Text = "Database Path:",
                    Font = SystemFonts.Bold()
                },
                
                CreateDatabasePathSection(),
                
                // Status/Info
                _statusLabel,
                
                new Panel { Height = 1, BackgroundColor = Color.FromArgb(200, 200, 200) },
                
                // Insert Options Section
                new Label
                {
                    Text = "Insert Options:",
                    Font = SystemFonts.Bold()
                },
                
                CreateInsertOptionsSection(),
                
                new StackLayoutItem(null, true), // Spacer
                
                // Buttons
                CreateButtonPanel()
            }
        };

        Content = layout;
    }

    private Control CreateDatabasePathSection()
    {
        var currentPath = _settingsService.GetSettings().BaseServerPath;
        
        _basePathTextBox = new TextBox
        {
            Text = currentPath,
            PlaceholderText = @"M:\Proiectare\__SCAN 3D Produse\__BOSCH\__NEW DB__"
        };
        
        var browseButton = new Button
        {
            Text = "Browse...",
            Width = 100
        };
        browseButton.Click += OnBrowseClicked;
        
        var testButton = new Button
        {
            Text = "Test Connection",
            Width = 120
        };
        testButton.Click += OnTestConnection;
        
        var pathLayout = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Items =
            {
                new StackLayoutItem(_basePathTextBox, true),
                browseButton
            }
        };
        
        var sectionLayout = new StackLayout
        {
            Spacing = 8,
            Items =
            {
                pathLayout,
                testButton,
                new Label
                {
                    Text = "Expected structure: [BasePath]/DIY or PRO/[Categories]/[Products]",
                    TextColor = Color.FromArgb(120, 120, 120),
                    Font = new Font(SystemFont.Default, 9)
                }
            }
        };
        
        return sectionLayout;
    }

    private Control CreateInsertOptionsSection()
    {
        var settings = _settingsService.GetSettings();
        
        // External File section
        _readLinkedBlocksCheckBox = new CheckBox
        {
            Text = "Read linked blocks from the file",
            Checked = settings.InsertReadLinkedBlocks
        };
        
        // Block Definition Type
        _blockTypeLinkedRadio = new RadioButton
        {
            Text = "Linked (references external file, smaller file size)",
            Checked = settings.InsertBlockType == "Linked"
        };
        
        _blockTypeEmbeddedRadio = new RadioButton(_blockTypeLinkedRadio)
        {
            Text = "Embedded (geometry copied into document)",
            Checked = settings.InsertBlockType == "Embedded"
        };
        
        // Layer Style
        _layerStyleActiveRadio = new RadioButton
        {
            Text = "Active",
            Checked = settings.InsertLayerStyle == "Active"
        };
        
        _layerStyleReferenceRadio = new RadioButton(_layerStyleActiveRadio)
        {
            Text = "Reference",
            Checked = settings.InsertLayerStyle == "Reference"
        };
        
        // Insert As
        _insertAsBlockRadio = new RadioButton
        {
            Text = "Block instance",
            Checked = settings.InsertAs == "BlockInstance"
        };
        
        _insertAsGroupRadio = new RadioButton(_insertAsBlockRadio)
        {
            Text = "As Group",
            Checked = settings.InsertAs == "Group"
        };
        
        var insertOptionsLayout = new StackLayout
        {
            Spacing = 10,
            Items =
            {
                // External File
                new Label
                {
                    Text = "External File:",
                    Font = new Font(SystemFont.Default, 9),
                    TextColor = Color.FromArgb(100, 100, 100)
                },
                _readLinkedBlocksCheckBox,
                
                // Block Definition Type
                new Label
                {
                    Text = "Block Definition Type:",
                    Font = new Font(SystemFont.Default, 9),
                    TextColor = Color.FromArgb(100, 100, 100)
                },
                new StackLayout
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 15,
                    Items = { _blockTypeLinkedRadio, _blockTypeEmbeddedRadio }
                },
                
                // Layer Style
                new Label
                {
                    Text = "Layer Style:",
                    Font = new Font(SystemFont.Default, 9),
                    TextColor = Color.FromArgb(100, 100, 100)
                },
                new StackLayout
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 15,
                    Items = { _layerStyleActiveRadio, _layerStyleReferenceRadio }
                },
                
                // Insert As
                new Label
                {
                    Text = "Insert As:",
                    Font = new Font(SystemFont.Default, 9),
                    TextColor = Color.FromArgb(100, 100, 100)
                },
                new StackLayout
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 15,
                    Items = { _insertAsBlockRadio, _insertAsGroupRadio }
                }
            }
        };
        
        return insertOptionsLayout;
    }

    private Control CreateButtonPanel()
    {
        var saveButton = new Button
        {
            Text = "Save",
            Width = 100
        };
        saveButton.Click += OnSaveClicked;
        
        var cancelButton = new Button
        {
            Text = "Cancel",
            Width = 100
        };
        cancelButton.Click += OnCancelClicked;
        
        var buttonLayout = new StackLayout
        {
            Orientation = Orientation.Horizontal,
            Spacing = 10,
            HorizontalContentAlignment = HorizontalAlignment.Right,
            Items = { saveButton, cancelButton }
        };
        
        return buttonLayout;
    }

    private void OnBrowseClicked(object? sender, EventArgs e)
    {
        var dialog = new SelectFolderDialog
        {
            Title = "Select Database Base Path",
            Directory = _basePathTextBox.Text
        };
        
        if (dialog.ShowDialog(this) == DialogResult.Ok)
        {
            _basePathTextBox.Text = dialog.Directory;
        }
    }

    private void OnTestConnection(object? sender, EventArgs e)
    {
        var path = _basePathTextBox.Text;
        
        if (string.IsNullOrWhiteSpace(path))
        {
            _statusLabel.Text = "⚠ Please enter a path first";
            _statusLabel.TextColor = Colors.Orange;
            return;
        }
        
        if (!System.IO.Directory.Exists(path))
        {
            _statusLabel.Text = $"❌ Path not found: {path}";
            _statusLabel.TextColor = Colors.Red;
            return;
        }
        
        // Check for expected structure (DIY or PRO folders)
        // Structure can be:
        // 1. [BasePath]/DIY and [BasePath]/PRO (direct)
        // 2. [BasePath]/Tools and Holders/DIY and [BasePath]/Tools and Holders/PRO
        var diyPath = System.IO.Path.Combine(path, "DIY");
        var proPath = System.IO.Path.Combine(path, "PRO");
        var toolsHoldersDiy = System.IO.Path.Combine(path, "Tools and Holders", "DIY");
        var toolsHoldersPro = System.IO.Path.Combine(path, "Tools and Holders", "PRO");
        
        bool hasDirectRanges = System.IO.Directory.Exists(diyPath) || System.IO.Directory.Exists(proPath);
        bool hasToolsHoldersStructure = System.IO.Directory.Exists(toolsHoldersDiy) || System.IO.Directory.Exists(toolsHoldersPro);
        
        if (hasDirectRanges || hasToolsHoldersStructure)
        {
            var structure = hasDirectRanges ? "DIY/PRO" : "Tools and Holders/DIY/PRO";
            _statusLabel.Text = $"✓ Valid database path - {structure} structure found";
            _statusLabel.TextColor = Colors.Green;
        }
        else
        {
            _statusLabel.Text = "⚠ Path exists but no DIY/PRO folders found. Is this the correct database root?";
            _statusLabel.TextColor = Colors.Orange;
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        var newPath = _basePathTextBox.Text?.Trim();
        
        if (string.IsNullOrWhiteSpace(newPath))
        {
            MessageBox.Show("Please enter a valid path", "Invalid Input", MessageBoxType.Warning);
            return;
        }
        
        // Save settings
        var settings = _settingsService.GetSettings();
        settings.BaseServerPath = newPath;
        
        // Save insert options
        settings.InsertReadLinkedBlocks = _readLinkedBlocksCheckBox.Checked ?? false;
        settings.InsertBlockType = _blockTypeLinkedRadio.Checked == true ? "Linked" : "Embedded";
        settings.InsertLayerStyle = _layerStyleActiveRadio.Checked == true ? "Active" : "Reference";
        settings.InsertAs = _insertAsBlockRadio.Checked == true ? "BlockInstance" : "Group";
        
        // IMPORTANT: await the save to complete before closing dialog
        await _settingsService.UpdateAsync(settings);
        
        Close(true);
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        Close(false);
    }
}
