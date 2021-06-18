using Inventory.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inventory.Extensions
{
    public static class RoleListExtension
    {
        public static IEnumerable<SelectListItem> ToSelectListItem<T>(this IEnumerable<T> items, string selectedValue)
        {
            return from item in items
                   select new SelectListItem
                   {
                       Text = item.GetPropertyValue("NormalizedName"),
                       Value = item.GetPropertyValue("Id"),
                       Selected = item.GetPropertyValue("Id").Equals(selectedValue)
                   };
        }

        public static IEnumerable<SelectListItem> ToSelectListItemEmp<T>(this IEnumerable<T> items, string selectedValue)
        {
            return from item in items
                   select new SelectListItem
                   {
                       Text = item.GetPropertyValue("Nombre"),
                       Value = item.GetPropertyValue("Id"),
                       Selected = item.GetPropertyValue("Id").Equals(selectedValue)
                   };
        }

        public static IEnumerable<SelectListItem> ToSelectListItemWorkOrder<T>(this IEnumerable<T> items, string selectedValue)
        {
            return from item in items
                   select new SelectListItem
                   {
                       Text = "OT "+ item.GetPropertyValue("woId")+ " - Empresa "+ item.GetPropertyValue("Empresa")+
                       " - OC " + item.GetPropertyValue("oc"),
                       Value = item.GetPropertyValue("woId"),
                       Selected = item.GetPropertyValue("woId").Equals(selectedValue)
                   };
        }
    }
}
