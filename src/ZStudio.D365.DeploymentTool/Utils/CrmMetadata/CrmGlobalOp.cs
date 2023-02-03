using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk;

namespace ZD365DT.DeploymentTool.Utils.CrmMetadata
{
    public class CrmGlobalOpCollection : ICollection<CrmGlobalOp>
    {
        private Dictionary<string, CrmGlobalOp> _list = new Dictionary<string, CrmGlobalOp>();

        public int Count
        {
            get
            {
                return _list.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(CrmGlobalOp item)
        {
            _list.Add(item.Name, item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(CrmGlobalOp item)
        {
            foreach (var o in _list)
            {
                if (o.Key == item.Name)
                    return true;
            }
            return false;
        }

        public void CopyTo(CrmGlobalOp[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<CrmGlobalOp> GetEnumerator()
        {
            return _list.Values.GetEnumerator();
        }

        public bool Remove(CrmGlobalOp item)
        {
            if (Contains(item))
            {
                _list.Remove(item.Name);
                return true;
            }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_list.Values).GetEnumerator();
        }
        
        /// <summary>
        /// Return the list as string summary
        /// </summary>
        /// <returns></returns>
        public string ToOutput()
        {
            StringBuilder sb = new StringBuilder();
            foreach (CrmGlobalOp op in this)
            {
                sb.AppendLine();
                sb.Append(op.ToOutput());
                sb.Append(op.Options.ToOutput());
            }
            return sb.ToString();
        }
    }

    public class CrmGlobalOp : CrmMetadataObject
    {
        /// <summary>
        /// Logical Name, always in lower case
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Schema Name, the physical name in SQL Database
        /// </summary>
        public string SchemaName { get; set; }

        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsSystem { get; set; }
        public int LanguageCode { get; set; }

        public CrmOptionCollection Options { get; set; }

        /// <summary>
        /// Return the summary of this object
        /// </summary>
        /// <returns></returns>
        public string ToOutput()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Global Optionset Name: {0}; Display Name: {1}; Language Code: {2}", Name, DisplayName, LanguageCode);
            sb.AppendLine();
            sb.AppendFormat("Total Options: {0}", Options.Count);
            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// Merge the optionset provided into this defined copy to decide what action to take, if the display name or description has change update is require
        /// </summary>
        /// <param name="options"></param>
        public void MergeWith(OptionSetMetadata opt)
        {
            //check if any value has change
            if (opt?.DisplayName?.UserLocalizedLabel?.Label != DisplayName || opt?.Description?.UserLocalizedLabel?.Label != Description)
            {
                Action = InstallationAction.Update;
                LogMessage = string.Format("Update: {0}: {1} to {2} (description might have changed)", Name, opt.DisplayName.UserLocalizedLabel.Label, DisplayName);
            }
            else
            {
                Action = InstallationAction.NoAction;
                LogMessage = string.Format("No Action: {0}", Name);
            }
            Logger.LogVerbose(LogMessage);
        }
    }

    public class CrmOptionCollection : ICollection<CrmOption>
    {
        private Dictionary<int, CrmOption> _list = new Dictionary<int, CrmOption>();

        public int Count
        {
            get
            {
                return _list.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(CrmOption item)
        {
            _list.Add(item.Value, item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(CrmOption item)
        {
            foreach (var o in _list)
            {
                if (o.Key == item.Value)
                    return true;
            }
            return false;
        }

        public bool Contains(int value)
        {
            foreach (var o in _list)
            {
                if (o.Key == value)
                    return true;
            }
            return false;
        }

        public void CopyTo(CrmOption[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<CrmOption> GetEnumerator()
        {
            return _list.Values.GetEnumerator();
        }

        public bool Remove(CrmOption item)
        {
            if (Contains(item))
            {
                _list.Remove(item.Value);
                return true;
            }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_list.Values).GetEnumerator();
        }

        /// <summary>
        /// Return the list as string summary
        /// </summary>
        /// <returns></returns>
        public string ToOutput()
        {
            StringBuilder sb = new StringBuilder();
            foreach (CrmOption o in this)
            {
                sb.Append(o.ToOutput());
            }
            return sb.ToString();
        }

        public OptionMetadataCollection ToOptionMetadataCollection(bool excludeDelete = false)
        {
            OptionMetadataCollection c = new OptionMetadataCollection();
            foreach (var op in _list.OrderBy(x => x.Value.Order))
            {
                if (!excludeDelete)
                    c.Add(op.Value.ToOptionMetadata());
                else if (op.Value.Action != InstallationAction.Delete)
                    c.Add(op.Value.ToOptionMetadata());
            }
            return c;
        }

        public OptionMetadata[] ToOptionMetadataArray(bool excludeDelete = false)
        {
            return this.ToOptionMetadataCollection(excludeDelete).ToArray();
        }

        public CrmOption[] OptionToCreate()
        {
            return this.Where(x => x.Action == InstallationAction.Create).ToArray();
        }

        public CrmOption[] OptionToUpdate()
        {
            return this.Where(x => x.Action == InstallationAction.Update).ToArray();
        }

        public CrmOption[] OptionToDelete()
        {
            return this.Where(x => x.Action == InstallationAction.Delete).ToArray();
        }

        /// <summary>
        /// Merge the options provided into this defined copy to decide what action to take on each option value, if the optionset does not exists in the XML, it will be deleted from CRM, if it exists, an update will be perform if something has changed, if optionset in the XML is new, it will be created
        /// </summary>
        /// <param name="options"></param>
        public void MergeWith(OptionMetadata[] options)
        {
            foreach (OptionMetadata opt in options)
            {
                if (_list.ContainsKey(opt.Value.Value))
                {
                    //check if any value has change
                    if (opt.Label.UserLocalizedLabel.Label != _list[opt.Value.Value].Label || (opt.Description != null && opt.Description.UserLocalizedLabel != null && opt.Description.UserLocalizedLabel.Label != _list[opt.Value.Value].Description))
                    {
                        _list[opt.Value.Value].Action = InstallationAction.Update;
                        _list[opt.Value.Value].LogMessage = string.Format("Update: {0}: {1} - {2} to {3} - {4}", opt.Value.Value, opt.Label.UserLocalizedLabel.Label, (opt.Description != null && opt.Description.UserLocalizedLabel != null) ? opt.Description.UserLocalizedLabel.Label : "null", _list[opt.Value.Value].Label, _list[opt.Value.Value].Description);
                    }
                    else
                    {
                        _list[opt.Value.Value].Action = InstallationAction.NoAction;
                        _list[opt.Value.Value].LogMessage = string.Format("No Action: {0} - {1}", opt.Value.Value, opt.Label.UserLocalizedLabel.Label);
                    }
                }
                else
                {
                    //a value in the system does not exist in the XML, delete it
                    _list.Add(opt.Value.Value, new CrmOption()
                    {
                        Value = opt.Value.Value,
                        Label = opt.Label.UserLocalizedLabel.Label,
                        Action = InstallationAction.Delete,
                        LogMessage = string.Format("Delete: {0} - {1}", opt.Value.Value, opt.Label.UserLocalizedLabel.Label)
                    });
                }
                Logger.LogVerbose(_list[opt.Value.Value].LogMessage);
            }

            //log all options to create, if any
            if (OptionToCreate() != null && OptionToCreate().Length > 0)
            {
                foreach (CrmOption opToAction in OptionToCreate())
                {
                    opToAction.LogMessage = string.Format("Create: {0} - {1}", opToAction.Value, opToAction.Label);
                    Logger.LogVerbose(opToAction.LogMessage);
                }
            }
        }
    }
    
    public class CrmOption : CrmMetadataObject
    {
        public int Value { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public int Order { get; set; }
        public int LanguageCode { get; set; }

        public OptionMetadata ToOptionMetadata()
        {
            return new OptionMetadata(new Label(this.Label, LanguageCode), this.Value)
            {
                Description = new Label(this.Description, LanguageCode)
            };
        }

        /// <summary>
        /// Return the summary of this object
        /// </summary>
        /// <returns></returns>
        public string ToOutput()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Value: {0}; Label: {1}; Description: {2}; Color: {3}; Order: {4}", Value, Label, Description, Color, Order);
            sb.AppendLine();
            return sb.ToString();
        }
    }
}