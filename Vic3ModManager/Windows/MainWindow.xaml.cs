﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Vic3ModManager.Essentials;

namespace Vic3ModManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dictionary<string, Func<CustomPage>> pages = [];
        private CustomPage? currentPage;

        public static MainWindow Instance { get; private set; } = new();


        public MainWindow()
        {
            InitializeComponent();

            InitializePages();

            Closing += MainWindow_Closing;
            ModManager.OnModSwitched += ModManager_OnModSwitched;

            Settings.IsEnabled = false;

            Instance = this;
        }


        private void SwtichToOtherMod(Mod mod)
        {
            var needSave = MessageBox.Show("Do you want save changes before switching?", "Save", MessageBoxButton.YesNo);

            if (needSave == MessageBoxResult.Yes)
            {
                ModManager.SaveCurrentMod();
            }

            ModManager.SwitchMod(mod);
            ReloadCurrentPage();
        }

        private void ReloadCurrentPage()
        {
            if (currentPage != null)
            {
                ChangePage(currentPage.Name);
            }
        }

        private void AnimateModChooser(bool hasMod)
        {
            string storyboardName = hasMod ? "HasModAnimation" : "HasNoModAnimation";
            if (FindResource(storyboardName) is Storyboard storyboard)
            {
                storyboard.Begin(ModChooser);
            }
        }

        private void InitializePages()
        {
            AddPage(nameof(Home), () => new Home());
            AddPage(nameof(MusicManager), () => new MusicManager());
            AddPage(nameof(LocalizationManager), () => new LocalizationManager());
            AddPage(nameof(Export), () => new Export());

            GenerateNavigationButtons();

            ChangePage(nameof(Home));
        }

        private void AddPage(string name, Func<CustomPage> pageFactory)
        {
            pages.Add(name, pageFactory);
        }

        private void GenerateNavigationButtons()
        {
            foreach (var page in pages)
            {
                AddNavigationButton(page.Key);
            }
        }

        private void AddNavigationButton(string pageName)
        {
            var navButton = new Button
            {
                Content = StringHelpers.ConvertCamelCaseToSpaces(pageName),
                Style = (Style)FindResource("NavigationButtonStyle")
            };
            navButton.Click += NavButton_Click;
            Navigations.Children.Add(navButton);
        }

        private void UpdateModChooserState()
        {
            bool hasMod = ModManager.CurrentMod != null;

            AnimateModChooser(hasMod);
            UpdateModChooserBlock(hasMod);
            UpdateTopBorderContextMenu(hasMod);
        }

        private void UpdateModChooserBlock(bool hasMod)
        {
            ModChooserBlock.Text = hasMod ? ModManager.CurrentMod.Name : "";
        }

        private void UpdateTopBorderContextMenu(bool hasMod)
        {
            ModChooser.ContextMenu = hasMod ? CreateModContextMenu() : null;
        }

        private ContextMenu? CreateModContextMenu()
        {
            ContextMenu? contextMenu = null;

            if (ModManager.AllMods.Count > 1)
            {
                contextMenu = new();
                foreach (var mod in ModManager.AllMods)
                {
                    if (mod == ModManager.CurrentMod) continue;

                    MenuItem menuItem = new()
                    {
                        Header = mod.Name
                    };
                    menuItem.Click += (object sender, RoutedEventArgs e) =>
                    {
                        SwtichToOtherMod(mod);
                    };

                    contextMenu.Items.Add(menuItem);
                }
            }

            return contextMenu;
        }

        private void ModChooserBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && ModChooser.ContextMenu != null)
            {
                ModChooser.ContextMenu.IsOpen = true;
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
            }
        }

        public void RefreshNavigationButtons()
        {
            for (int i = 0; i < Navigations.Children.Count; i++)
            {
                // Skipping if unable to find a button
                if (Navigations.Children[i] is not Button navButton) continue;

                // Disabling navigation if there are no mods
                // To avoid crashes
                if (ModManager.AllMods.Count == 0)
                {
                    navButton.IsEnabled = false;
                    continue;
                }

                if (i == GetPageIndex(currentPage.Name))
                {
                    navButton.IsEnabled = false;
                }
                else
                {
                    navButton.IsEnabled |= true;
                }
            }
        }

        public void ToggleNavigationButtons(bool enabled)
        {
            foreach (var navButton in Navigations.Children)
            {
                if (navButton is Button button)
                {
                    button.IsEnabled = enabled;
                }
            }
        }

        private int GetPageIndex(string key)
        {
            var keys = pages.Keys.ToList();
            return keys.IndexOf(key);
        }


        private void ChangePage(string pageName)
        {
            pageName = pageName.Replace(" ", "");
            if (pages.TryGetValue(pageName, out var pageFactory))
            {
                // Unsubscribe from the current page's RequestPageChange event
                if (currentPage != null)
                {
                    currentPage.RequestPageChange -= ChangePage;
                }

                currentPage = pageFactory();
                currentPage.RequestPageChange += ChangePage;
                MainFrame.Content = currentPage;
            }
        }


        private void SwitchProjectSaveButtonVisibility()
        {
            if (ModManager.CurrentMod == null)
            {
                SaveProjectButton.Visibility = Visibility.Hidden;
            }
            else
            {
                SaveProjectButton.Visibility = Visibility.Visible;
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (ModManager.CurrentMod != null)
            {
                var result = MessageBox.Show("Do you want to save the current mod before exiting?", "Save mod?", MessageBoxButton.YesNoCancel);

                if (result == MessageBoxResult.Yes)
                {
                    ModManager.SaveCurrentMod();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }

            Application.Current.Shutdown();
        }

        private void MainFrame_ContentRendered(object sender, EventArgs e)
        {
            RefreshNavigationButtons();

            SwitchProjectSaveButtonVisibility();
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            ChangePage(button.Content.ToString());
        }

        private void SaveProjectButton_Click(object sender, RoutedEventArgs e)
        {
            ModManager.SaveCurrentMod();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.IsEnabled = true;
        }


        private void ModManager_OnModSwitched(object sender, EventArgs e)
        {
            UpdateModChooserState();
        }
    }
}
