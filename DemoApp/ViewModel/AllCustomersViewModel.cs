using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DemoApp.DataAccess;
using DemoApp.Properties;

namespace DemoApp.ViewModel
{
    /// <summary>
    /// Represents a container of CustomerViewModel objects
    /// that has support for staying synchronized with the
    /// CustomerRepository.  This class also provides information
    /// related to multiple selected customers.
    /// </summary>
    public class AllCustomersViewModel : WorkspaceViewModel
    {
        #region Fields

        readonly CustomerRepository _customerRepository;

        #endregion // Fields

        #region Constructor

        public AllCustomersViewModel(CustomerRepository customerRepository)
        {
            if (customerRepository == null)
                throw new ArgumentNullException("customerRepository");

            base.DisplayName = Strings.AllCustomersViewModel_DisplayName;            

            _customerRepository = customerRepository;

            // Subscribe for notifications of when a new customer is saved.
            _customerRepository.CustomerAdded += this.OnCustomerAddedToRepository;

            // Populate the AllCustomers collection with CustomerViewModels.
            this.CreateAllCustomers();              
        }

        void CreateAllCustomers()
        {
            List<CustomerViewModel> all =
                (from cust in _customerRepository.GetCustomers()
                 select new CustomerViewModel(cust, _customerRepository)).ToList();

            foreach (CustomerViewModel cvm in all)
                cvm.PropertyChanged += this.OnCustomerViewModelPropertyChanged;

            this.AllCustomers = new ObservableCollection<CustomerViewModel>(all);
            this.AllCustomers.CollectionChanged += this.OnCollectionChanged;
        }

        #endregion // Constructor

        #region Public Interface

        /// <summary>
        /// Returns a collection of all the CustomerViewModel objects.
        /// </summary>
        public ObservableCollection<CustomerViewModel> AllCustomers { get; private set; }

        /// <summary>
        /// Returns the total sales sum of all selected customers.
        /// </summary>
        public double TotalSelectedSales
        {
            get
            {
                return this.AllCustomers.Sum(
                    custVM => custVM.IsSelected ? custVM.TotalSales : 0.0);
            }
        }

        #endregion // Public Interface

        #region  Base Class Overrides

        protected override void OnDispose()
        {
            foreach (CustomerViewModel custVM in this.AllCustomers)
                custVM.Dispose();

            this.AllCustomers.Clear();
            this.AllCustomers.CollectionChanged -= this.OnCollectionChanged;

            _customerRepository.CustomerAdded -= this.OnCustomerAddedToRepository;
        }

        #endregion // Base Class Overrides

        #region Event Handling Methods

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
                foreach (CustomerViewModel custVM in e.NewItems)
                    custVM.PropertyChanged += this.OnCustomerViewModelPropertyChanged;

            if (e.OldItems != null && e.OldItems.Count != 0)
                foreach (CustomerViewModel custVM in e.OldItems)
                    custVM.PropertyChanged -= this.OnCustomerViewModelPropertyChanged;
        }

        void OnCustomerViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string IsSelected = "IsSelected";

            // Make sure that the property name we're referencing is valid.
            // This is a debugging technique, and does not execute in a Release build.
            (sender as CustomerViewModel).VerifyPropertyName(IsSelected);

            // When a customer is selected or unselected, we must let the
            // world know that the TotalSelectedSales property has changed,
            // so that it will be queried again for a new value.
            if (e.PropertyName == IsSelected)
                this.OnPropertyChanged("TotalSelectedSales");
        }

        void OnCustomerAddedToRepository(object sender, CustomerAddedEventArgs e)
        {
            var viewModel = new CustomerViewModel(e.NewCustomer, _customerRepository);
            this.AllCustomers.Add(viewModel);
        }

        #endregion // Event Handling Methods
    }
}