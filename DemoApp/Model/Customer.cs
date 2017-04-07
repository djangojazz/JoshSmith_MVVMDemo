using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using DemoApp.Properties;

namespace DemoApp.Model
{
    /// <summary>
    /// Represents a customer of a company.  This class
    /// has built-in validation logic. It is wrapped
    /// by the CustomerViewModel class, which enables it to
    /// be easily displayed and edited by a WPF user interface.
    /// </summary>
    public class Customer : IDataErrorInfo
    {
        #region Creation

        public static Customer CreateNewCustomer()
        {
            return new Customer();
        }

        public static Customer CreateCustomer(
            double totalSales,
            string firstName,
            string lastName,
            bool isCompany,
            string email)
        {
            return new Customer
            {
                TotalSales = totalSales,
                FirstName = firstName,
                LastName = lastName,
                IsCompany = isCompany,
                Email = email
            };
        }

        protected Customer()
        {
        }

        #endregion // Creation

        #region State Properties

        /// <summary>
        /// Gets/sets the e-mail address for the customer.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets/sets the customer's first name.  If this customer is a 
        /// company, this value stores the company's name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets/sets whether the customer is a company or a person.
        /// The default value is false.
        /// </summary>
        public bool IsCompany { get; set; }

        /// <summary>
        /// Gets/sets the customer's last name.  If this customer is a 
        /// company, this value is not set.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Returns the total amount of money spent by the customer.
        /// </summary>
        public double TotalSales { get; private set; }

        #endregion // State Properties

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error { get { return null; } }

        string IDataErrorInfo.this[string propertyName]
        {
            get { return this.GetValidationError(propertyName); }
        }

        #endregion // IDataErrorInfo Members

        #region Validation

        /// <summary>
        /// Returns true if this object has no validation errors.
        /// </summary>
        public bool IsValid
        {
            get
            {
                foreach (string property in ValidatedProperties)
                    if (GetValidationError(property) != null)
                        return false;

                return true;
            }
        }

        static readonly string[] ValidatedProperties = 
        { 
            "Email", 
            "FirstName", 
            "LastName",
        };

        string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0)
                return null;

            string error = null;

            switch (propertyName)
            {
                case "Email":
                    error = this.ValidateEmail();
                    break;

                case "FirstName":
                    error = this.ValidateFirstName();
                    break;

                case "LastName":
                    error = this.ValidateLastName();
                    break;

                default:
                    Debug.Fail("Unexpected property being validated on Customer: " + propertyName);
                    break;
            }

            return error;
        }

        string ValidateEmail()
        {
            if (IsStringMissing(this.Email))
            {
                return Strings.Customer_Error_MissingEmail;
            }
            else if (!IsValidEmailAddress(this.Email))
            {
                return Strings.Customer_Error_InvalidEmail;
            }
            return null;
        }

        string ValidateFirstName()
        {
            if (IsStringMissing(this.FirstName))
            {
                return Strings.Customer_Error_MissingFirstName;
            }
            return null;
        }

        string ValidateLastName()
        {
            if (this.IsCompany)
            {
                if (!IsStringMissing(this.LastName))
                    return Strings.Customer_Error_CompanyHasNoLastName;
            }
            else
            {
                if (IsStringMissing(this.LastName))
                    return Strings.Customer_Error_MissingLastName;
            }
            return null;
        }

        static bool IsStringMissing(string value)
        {
            return
                String.IsNullOrEmpty(value) ||
                value.Trim() == String.Empty;
        }

        static bool IsValidEmailAddress(string email)
        {
            if (IsStringMissing(email))
                return false;

            // This regex pattern came from: http://haacked.com/archive/2007/08/21/i-knew-how-to-validate-an-email-address-until-i.aspx
            string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }

        #endregion // Validation
    }
}