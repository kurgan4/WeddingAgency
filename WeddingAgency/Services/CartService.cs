using System;
using System.Collections.Generic;
using System.Linq;

namespace WeddingAgency.Services
{
    public class CartItem
    {
        public CatalogItems ProductOrService { get; set; }
        public Partners Partner { get; set; }
        public HolidayProducts HolidayProduct { get; set; } 
        public int Quantity { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }

    public static class CartService
    {
        public static List<CartItem> Items { get; } = new List<CartItem>();

        public static void Add(CatalogItems item, int quantity = 1, TimeSpan? start = null, TimeSpan? end = null, Partners partner = null, HolidayProducts holidayProduct = null)
        {
            if (holidayProduct != null)
            {
                var existingProd = Items.FirstOrDefault(i => i.HolidayProduct != null && i.HolidayProduct.ProductID == holidayProduct.ProductID);
                if (existingProd != null) existingProd.Quantity += quantity;
                else Items.Add(new CartItem { HolidayProduct = holidayProduct, Quantity = quantity });
            }
            else if (item != null)
            {
                var existingItem = Items.FirstOrDefault(i => i.ProductOrService != null && i.ProductOrService.ItemID == item.ItemID);
                if (existingItem != null) existingItem.Quantity += quantity;
                else Items.Add(new CartItem { ProductOrService = item, Quantity = quantity });
            }
            else if (partner != null)
            {
                var existingPartner = Items.FirstOrDefault(i => i.Partner != null && i.Partner.PartnerID == partner.PartnerID);
                if (existingPartner != null)
                {
                    existingPartner.StartTime = start;
                    existingPartner.EndTime = end;
                }
                else Items.Add(new CartItem { Partner = partner, Quantity = 1, StartTime = start, EndTime = end });
            }
        }

        public static void Remove(int? itemId, int? partnerId, int? holidayProductId)
        {
            if (holidayProductId.HasValue)
            {
                var prod = Items.FirstOrDefault(i => i.HolidayProduct != null && i.HolidayProduct.ProductID == holidayProductId.Value);
                if (prod != null) Items.Remove(prod);
            }
            else if (itemId.HasValue)
            {
                var item = Items.FirstOrDefault(i => i.ProductOrService != null && i.ProductOrService.ItemID == itemId.Value);
                if (item != null) Items.Remove(item);
            }
            else if (partnerId.HasValue)
            {
                var partner = Items.FirstOrDefault(i => i.Partner != null && i.Partner.PartnerID == partnerId.Value);
                if (partner != null) Items.Remove(partner);
            }
        }

        public static decimal GetTotalSum()
        {
            decimal total = 0;
            foreach (var item in Items)
            {
                if (item.HolidayProduct != null)
                {
                    total += item.HolidayProduct.Price * item.Quantity;
                }
                else if (item.Partner != null)
                {
                    if (item.StartTime.HasValue && item.EndTime.HasValue)
                    {
                        int hours = (int)Math.Ceiling((item.EndTime.Value - item.StartTime.Value).TotalHours);
                        if (hours <= 0) hours = 1;
                        total += item.Partner.PersonalPrice * hours;
                    }
                    else total += item.Partner.PersonalPrice;
                }
                else if (item.ProductOrService != null)
                {
                    total += item.ProductOrService.BasePrice * item.Quantity;
                }
            }
            return total;
        }

        public static void Clear()
        {
            Items.Clear();
        }

        public static bool IsServiceAvailable(int partnerId, DateTime date, TimeSpan start, TimeSpan end)
        {
            bool isBusy = Core.Context.EventItems.Any(ei =>
                ei.PartnerID == partnerId &&
                ei.Events.EventDate == date &&
                ei.Events.StatusID != 5 &&
                ((start >= ei.StartTime && start < ei.EndTime) ||
                 (end > ei.StartTime && end <= ei.EndTime) ||
                 (start <= ei.StartTime && end >= ei.EndTime)));

            return !isBusy;
        }
    }
}
