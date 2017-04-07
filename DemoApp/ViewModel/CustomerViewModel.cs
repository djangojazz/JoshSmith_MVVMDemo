using System;
using System.ComponentModel;
using System.Windows.Input;
using DemoApp.DataAccess;
using DemoApp.Model;
using DemoApp.Properties;

namespace DemoApp.ViewModel
{
    /// <summary>
    /// A UI-friendly wrapper for a Customer object.
    /// </summary>
    public class CustomerViewModel : WorkspaceViewModel, IDataErrorInfo
    {
        #region Fields

        readonly Customer _customer;
        readonly CustomerRepository _customerRepository;
        string _customerType;
        string[] _customerTypeOptions;
        bool _isSelected;
        RelayCommand _saveCommand;

        #endregion // Fields

        #region Constructor

        public CustomerViewModel(Customer customer, CustomerRepository customerRepository)
        {
            _customer = customer ?? throw new ArgumentNullException("customer");
            _customerRepository = customerRepository ?? throw new ArgumentNullException("customerRepository");
            _customerType = Strings.CustomerViewModel_CustomerTypeOption_NotSpecified;
        }

        #endregion // Constructor

        #region Customer Properties

        public string Email
        {
            get { return _customer.Email; }
            set
            {
                if (value == _customer.Email)
                    return;

                _customer.Email = value;

                base.OnPropertyChanged("Email");
            }
        }

        public string FirstName
        {
            get { return _customer.FirstName; }
            set
            {
                if (value == _customer.FirstName)
                    return;

                _customer.FirstName = value;

                base.OnPropertyChanged("FirstName");
            }
        }

        public bool IsCompany
        {
            get { return _customer.IsCompany; }
        }

        public string LastName
        {
            get { return _customer.LastName; }
            set
            {
                if (value == _customer.LastName)
                    return;

                _customer.LastName = value;

                base.OnPropertyChanged("LastName");
            }
        }

        public double TotalSales
        {
            get { return _customer.TotalSales; }
        }

        #endregion // Customer Properties

        #region Presentation Properties

        /// <summary>
        /// Gets/sets a value that indicates what type of customer this is.
        /// This property maps to the IsCompany property of the Customer class,
        /// but also has support for an 'unselected' state.
        /// </summary>
        public string CustomerType
        {
            get { return _customerType; }
            set
            {
                if (value == _customerType || String.IsNullOrEmpty(value))
                    return;

                _customerType = value;

                if (_customerType == Strings.CustomerViewModel_CustomerTypeOption_Company)
                {
                    _customer.IsCompany = true;
                }
                else if (_customerType == Strings.CustomerViewModel_CustomerTypeOption_Person)
                {
                    _customer.IsCompany = false;
                }

                base.OnPropertyChanged("CustomerType");

                // Since this ViewModel object has knowledge of how to translate
                // a customer type (i.e. text) to a Customer object's IsCompany property,
                // it also must raise a property change notification when it changes
                // the value of IsCompany.  The LastName property is validated 
                // differently based on whether the customer is a company or not,
                // so the validation for the LastName property must execute now.
                base.OnPropertyChanged("LastName");
            }
        }

        /// <summary>
        /// Returns a list of strings used to populate the Customer Type selector.
        /// </summary>
        public string[] CustomerTypeOptions
        {
            get
            {
                if (_customerTypeOptions == null)
                {
                    _customerTypeOptions = new string[]
                    {
                        Strings.CustomerViewModel_CustomerTypeOption_NotSpecified,
                        Strings.CustomerViewModel_CustomerTypeOption_Person,
                        Strings.CustomerViewModel_CustomerTypeOption_Company
                    };
                }
                return _customerTypeOptions;
            }
        }

        public override string DisplayName
        {
            get
            {
                if (this.IsNewCustomer)
                {
                    return Strings.CustomerViewModel_DisplayName;
                }
                else if (_customer.IsCompany)
                {
                    return _customer.FirstName;
                }
                else
                {
                    return String.Format("{0}, {1}", _customer.LastName, _customer.FirstName);
                }
            }
        }

        /// <summary>
        /// Gets/sets whether this customer is selected in the UI.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected)
                    return;

                _isSelected = value;

                base.OnPropertyChanged("IsSelected");
            }
        }

        /// <summary>
        /// Returns a command that saves the customer.
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(
                        param => this.Save(),
                        param => this.CanSave
                        );
                }
                return _saveCommand;
            }
        }

        #endregion // Presentation Properties

        #region Public Methods

        /// <summary>
        /// Saves the customer to the repository.  This method is invoked by the SaveCommand.
        /// </summary>
        public void Save()
        {
            if (!_customer.IsValid)
                throw new InvalidOperationException(Strings.CustomerViewModel_Exception_CannotSave);

            if (this.IsNewCustomer)
                _customerRepository.AddCustomer(_customer);
            
            base.OnPropertyChanged("DisplayName");
        }

        #endregion // Public Methods

        #region Private Helpers

        /// <summary>
        /// Returns true if this customer was created by the user and it has not yet
        /// been saved to the customer repository.
        /// </summary>
        bool IsNewCustomer
        {
            get { return !_customerRepository.ContainsCustomer(_customer); }
        }

        /// <summary>
        /// Returns true if the customer is valid and can be saved.
        /// </summary>
        bool CanSave
        {
            get { return String.IsNullOrEmpty(this.ValidateCustomerType()) && _customer.IsValid; }
        }

        #endregion // Private Helpers

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get { return (_customer as IDataErrorInfo).Error; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                string error = null;

                if (propertyName == "CustomerType")
                {
                    // The IsCompany property of the Customer class 
                    // is Boolean, so it has no concept of being in
                    // an "unselected" state.  The CustomerViewModel
                    // class handles this mapping and validation.
                    error = this.ValidateCustomerType();
                }
                else
                {
                    error = (_customer as IDataErrorInfo)[propertyName];
                }

                // Dirty the commands registered with CommandManager,
                // such as our Save command, so that they are queried
                // to see if they can execute now.
                CommandManager.InvalidateRequerySuggested();

                return error;
            }
        }

        string ValidateCustomerType()
        {
            if (this.CustomerType == Strings.CustomerViewModel_CustomerTypeOption_Company ||
               this.CustomerType == Strings.CustomerViewModel_CustomerTypeOption_Person)
                return null;

            return Strings.CustomerViewModel_Error_MissingCustomerType;
        }

        #endregion // IDataErrorInfo Members
    }
}