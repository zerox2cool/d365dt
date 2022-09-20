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
    public class CrmManyToManyRelationCollection : ICollection<CrmManyToManyRelation>
    {
        private Dictionary<string, CrmManyToManyRelation> _list = new Dictionary<string, CrmManyToManyRelation>();

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

        public void Add(CrmManyToManyRelation item)
        {
            _list.Add(item.Name, item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(CrmManyToManyRelation item)
        {
            foreach (var o in _list)
            {
                if (o.Key == item.Name)
                    return true;
            }
            return false;
        }

        public void CopyTo(CrmManyToManyRelation[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<CrmManyToManyRelation> GetEnumerator()
        {
            return _list.Values.GetEnumerator();
        }

        public bool Remove(CrmManyToManyRelation item)
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
            foreach (CrmManyToManyRelation rel in this)
            {
                sb.AppendLine();
                sb.Append(rel.ToOutput());
            }
            return sb.ToString();
        }
    }

    public class CrmManyToManyRelation : CrmMetadataObject
    {
        /// <summary>
        /// Logical Name, always in lower case
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Schema Name, the physical name in SQL Database
        /// </summary>
        public string SchemaName { get; set; }

        public bool IsSystem { get; set; }
        public bool IsValidForAdvancedFind { get; set; }

        public CrmEntityRelation Entity1 { get; set; }
        public CrmEntityRelation Entity2 { get; set; }

        /// <summary>
        /// Return the summary of this object
        /// </summary>
        /// <returns></returns>
        public string ToOutput()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("N:N Relation Name: {0}; Searchable: {1};", Name, IsValidForAdvancedFind);
            sb.AppendLine();
            sb.AppendFormat("Entity 1: {0}", Entity1.ToOutput());
            sb.AppendFormat("Entity 2: {0}", Entity2.ToOutput());
            sb.AppendLine();
            return sb.ToString();
        }
    }

    public class CrmEntityRelation : CrmMetadataObject
    {
        public string Name { get; set; }
        public bool IsSystem { get; set; }
        public int LanguageCode { get; set; }
        public string CustomLabel { get; set; }
        public int MenuOrder { get; set; }

        public string MenuBehavior { private get; set; }
        public CrmAssociatedMenuBehavior CrmMenuBehavior
        {
            get
            {
                //by default return use collection name
                CrmAssociatedMenuBehavior result = CrmAssociatedMenuBehavior.UseCollectionName;
                if (Enum.TryParse<CrmAssociatedMenuBehavior>(MenuBehavior, out result))
                    return result;
                else
                    return CrmAssociatedMenuBehavior.UseCollectionName;
            }
        }
        public string MenuGroup { private get; set; }
        public CrmAssociatedMenuGroup CrmMenuGroup
        {
            get
            {
                //by default return details
                CrmAssociatedMenuGroup result = CrmAssociatedMenuGroup.Details;
                if (Enum.TryParse<CrmAssociatedMenuGroup>(MenuGroup, out result))
                    return result;
                else
                    return CrmAssociatedMenuGroup.Details;
            }
        }

        /// <summary>
        /// Return the summary of this object
        /// </summary>
        /// <returns></returns>
        public string ToOutput()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Name: {0}; Custom Label: {1}; MenuBehavior: {2}; MenuGroup: {3}; MenuOrder: {4}; LanguageCode: {5};", Name, CustomLabel, CrmMenuBehavior.ToString(), CrmMenuGroup.ToString(), MenuOrder, LanguageCode);
            sb.AppendLine();
            return sb.ToString();
        }
    }
}