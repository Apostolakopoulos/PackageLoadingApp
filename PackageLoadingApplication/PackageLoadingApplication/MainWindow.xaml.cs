using PackageLoadingApplication.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PackageLoadingApplication
{
    public partial class MainWindow : Window
    {
        // create 2 ObservableCollection so as to fill the 2 comboboxes with data
        ObservableCollection<PackageProduct> products = new ObservableCollection<PackageProduct>();
        ObservableCollection<PackageProduct> packages = new ObservableCollection<PackageProduct>();

        // create 3 ObservableCollection for the 3 listviews so to bind data with them
        ObservableCollection<PackageProduct> productListBoxList = new ObservableCollection<PackageProduct>();
        ObservableCollection<PackageProduct> packageListBoxList = new ObservableCollection<PackageProduct>();
        ObservableCollection<PackageProduct> parentListBoxList = new ObservableCollection<PackageProduct>();

        public MainWindow()
        {
            InitializeComponent();
            FillComboBoxes();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // the user has selected a product from the combobox
            if (ProductComboBox.SelectedItem != null)
            {
                // the user has entered quantity
                if (!string.IsNullOrEmpty(QuantityTextBox.Text))
                {
                    // get the selected type of package from the combo box
                    PackageProduct selectedProduct = (PackageProduct)ProductComboBox.SelectedItem;
                    // get the quantity
                    int quant = Convert.ToInt32(QuantityTextBox.Text);

                    // create the entity id dynamically
                    int maxEntityId = 0;
                    // check if the list already have item with the same product id
                    if (productListBoxList.Where(item => item.ProductId == selectedProduct.ProductId).Count() > 0)
                    {
                        // if the item exists get the max entity id
                        maxEntityId = productListBoxList.Where(item => item.ProductId == selectedProduct.ProductId).Max(t => t.EntityId);
                    }

                    for (int i = 1; i <= quant; i++)
                    {
                        maxEntityId += 1;
                        // create the new product and add it to the list
                        PackageProduct prod = new PackageProduct()
                        {
                            ProductId = selectedProduct.ProductId,
                            Descr = selectedProduct.Descr,
                            EntityId = maxEntityId,
                            HasParent = 0,
                            IsParent = 0,
                            EntityName = selectedProduct.Descr + " " + maxEntityId.ToString()
                        };
                        productListBoxList.Add(prod);
                    }

                    // bind the list to the listview
                    ProductListBox.ItemsSource = productListBoxList;
                    ProductListBox.Items.Refresh();
                }
                else
                {
                    MessageBox.Show("The quantity cannot be 0");
                }
            }
            else
            {
                MessageBox.Show("You have not selected a product");
            }

            // combo box selection back to default
            ProductComboBox.SelectedIndex = -1;
            QuantityTextBox.Text = "";           
        }

        // validate the data of the textbox
        private void QuantityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // the user can only enter 2 digit numbers
            if (QuantityTextBox.Text.Length < 2)
            {
                // the user can only enter numbers
                var textBox = sender as TextBox;
                e.Handled = Regex.IsMatch(e.Text, "[^0-9]+");
            }
            else
            {
                e.Handled = true;
            }              
        }

        // transfer the item to the productListBoxList
        private void AddToPackageButton_Click(object sender, RoutedEventArgs e)
        {
            // if there are no items in the list do nothing
            if (productListBoxList.Count == 0)
            {
                return;
            }

            // if the user has selected anything
            if (ProductListBox.SelectedItems != null)
            {
                // get all the selected items from packageListBoxList and add them to a new list
                List<PackageProduct> selectedProducts = new List<PackageProduct>();
                foreach (var item in ProductListBox.SelectedItems)
                {
                    selectedProducts.Add((PackageProduct)item);
                }

                foreach (var item in selectedProducts)
                {
                    // add the item to list
                    packageListBoxList.Add(item);

                    // remove item from OrderPackagingProductsList
                    productListBoxList.Remove(productListBoxList.Single(i => i.ProductId == item.ProductId && i.EntityId == item.EntityId));
                }

                // refresh the items in listview
                PackageListBox.ItemsSource = packageListBoxList;
                PackageListBox.Items.Refresh();

                ProductListBox.ItemsSource = productListBoxList;
                ProductListBox.Items.Refresh();
            }
            else
            {
                return;
            }
        }

        // remove the item from the packageListBoxList back to productListBoxList
        private void RemoveFromPackageButton_Click(object sender, RoutedEventArgs e)
        {
            // if there are no items in the list do nothing
            if (packageListBoxList.Count == 0)
            {
                return;
            }

            // if the user has selected items
            if (PackageListBox.SelectedItems != null)
            {
                // get all the selected items from packageListBoxList and add them to a new list
                List<PackageProduct> selectedProducts = new List<PackageProduct>();
                foreach (var item in PackageListBox.SelectedItems)
                {
                    selectedProducts.Add((PackageProduct)item);
                }

                foreach (var item in selectedProducts)
                {
                    // remove item from packageListBoxList
                    packageListBoxList.Remove(packageListBoxList.Single(i => i.ProductId == item.ProductId && i.EntityId == item.EntityId));

                    // add the item to list
                    productListBoxList.Add(item);
                }

                // refresh the items in listview
                PackageListBox.ItemsSource = packageListBoxList;
                PackageListBox.Items.Refresh();

                ProductListBox.ItemsSource = productListBoxList;
                ProductListBox.Items.Refresh();
            }
            else
            {
                return;
            }
        }

        // add parent package to parents list and remove it's children if they exist
        private void ClosePackageButton_Click(object sender, RoutedEventArgs e)
        {
            // check if the user has selected a parent package
            if (PackageComboBox.SelectedItem == null)
            {
                MessageBox.Show("You have not selected a package to fill");
                return;
            }

            // if the packageListBoxList has items
            if (packageListBoxList.Any())
            {
                // create the parent and add it's
                var parent = CreateParentPackage();

                // just get and copy the data to a temporary list
                List<PackageProduct> tempList = new List<PackageProduct>();
                tempList = parentListBoxList.Where(i => i.IsParent == 1).ToList();

                foreach (var item in tempList)
                {
                    foreach (var childPackage in parent.ContainedPackagesList)
                    {
                        // remove from the ParentPackageList the children of the package that contains them
                        if (item.ProductId == childPackage.ProductId && item.EntityId == childPackage.EntityId)
                        {
                            parentListBoxList.Remove(childPackage);
                        }
                    }
                }

                // add to ParentPackageList                
                parentListBoxList.Add(parent);
                // add to OrderPackagingProductsList
                productListBoxList.Add(parent);

                // bind list to listview
                ParentPackageListBox.ItemsSource = parentListBoxList;
                ParentPackageListBox.Items.Refresh();

                // clear the InPackageProductList
                packageListBoxList.Clear();
                PackageListBox.ItemsSource = packageListBoxList;
                PackageListBox.Items.Refresh();

                // combo box selection back to default
                PackageComboBox.SelectedIndex = -1;
            }
        }

        // if the parent inside the parent listbox is clicked
        private void TextBoxClick(object sender, MouseButtonEventArgs e)
        {
            // get the parent package and call the EditParent function
            StackPanel lsb = (StackPanel)sender;
            PackageProduct selectedParent = (PackageProduct)lsb.DataContext;

            // combo box selection back to default
            PackageComboBox.SelectedIndex = -1;

            EditParent(selectedParent);
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // functions
        private PackageProduct CreateParentPackage()
        {
            // get the selected item
            PackageProduct selectedParent = (PackageProduct)PackageComboBox.SelectedItem;
            // create the parent entity id
            int maxParentEntityId = 0;
            if (parentListBoxList != null && parentListBoxList.Any())
            {
                // check if the list already have item with the same product id
                if (productListBoxList.Where(item => item.ProductId == selectedParent.ProductId).Count() > 0)
                {
                    // if the item exists get the max entity id
                    maxParentEntityId = productListBoxList.Where(item => item.ProductId == selectedParent.ProductId).Max(t => t.EntityId);
                }
            }

            //create a new parent
            PackageProduct parent = new PackageProduct()
            {
                ProductId = selectedParent.ProductId,
                Descr = selectedParent.Descr,
                EntityId = maxParentEntityId + 1,
                HasParent = 0,
                IsParent = 1,
                EntityName = selectedParent.Descr + " " + (maxParentEntityId + 1).ToString(),
                ContainedPackagesList = new List<PackageProduct>()
            };

            // add the items in the inner list
            foreach (var item in packageListBoxList)
            {
                item.HasParent = 1;
                parent.ContainedPackagesList.Add(item);
            }

            return parent;
        }

        private void EditParent(PackageProduct selectedParent)
        {
            // if the in package list is empty
            if (PackageListBox.Items.Count == 0)
            {
                // remove from Order Packaging Products List
                productListBoxList.Remove(selectedParent);

                // remove item from Parent Package List
                parentListBoxList.Remove(selectedParent);

                // fill the In Package Product List
                foreach (var child in selectedParent.ContainedPackagesList)
                {
                    // add to In Package Product List
                    packageListBoxList.Add(child);

                    // if the child is a parent add it to the Parent list
                    if (child.IsParent == 1)
                    {
                        child.HasParent = 0;
                        parentListBoxList.Add(child);
                    }
                }

                // bind lists to listviews
                PackageListBox.ItemsSource = packageListBoxList;
                PackageListBox.Items.Refresh();

                ParentPackageListBox.ItemsSource = parentListBoxList;
                ParentPackageListBox.Items.Refresh();

                ProductListBox.ItemsSource = productListBoxList;
                ProductListBox.Items.Refresh();
            }
            else
            {
                MessageBox.Show("You have items inside some package please remove them ", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void FillComboBoxes()
        {
            // add data to the ObservableCollections
            products.Add(new PackageProduct { ProductId = 101, Descr = "Bag" });
            products.Add(new PackageProduct { ProductId = 102, Descr = "Package" });
            products.Add(new PackageProduct { ProductId = 103, Descr = "Box" });

            packages.Add(new PackageProduct { ProductId = 104, Descr = "Palette" });
            packages.Add(new PackageProduct { ProductId = 103, Descr = "Truck" });

            // bind data to the ComboBoxes
            ProductComboBox.ItemsSource = products;
            ProductComboBox.DisplayMemberPath = "Descr";

            PackageComboBox.ItemsSource = packages;
            PackageComboBox.DisplayMemberPath = "Descr";

        }
    }
}
