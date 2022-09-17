using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using System.Transactions;

namespace WebApplication1.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        private DataClasses1DataContext dt = new DataClasses1DataContext();
        public List<Cart> GetListCarts()//lấy DS giỏ hàng
        {
            List<Cart> carts = Session["Cart"] as List<Cart>;
            if (carts == null)//chưa có Sp nào trong gio hàng
            {
                carts = new List<Cart>();
                Session["Cart"] = carts;
            }
            return carts;
        }

        private int Count()
        {
            int n = 0;
            List<Cart> carts = Session["Cart"] as List<Cart>;
            if (carts != null)
            {
                n = carts.Sum(s=>s.Quantity);
            }
            return n;
        }
        private decimal Total()
        {
            decimal total=0;
            List<Cart> carts = Session["Cart"] as List<Cart>;
            if (carts != null)
            {
                total = carts.Sum(s => s.Total);
            }
            return total;
        }

        public ActionResult AddCart(int id)
        {
            List<Cart> carts = GetListCarts();//lấy DSGH
            Cart c = carts.Find(s => s.ProductID == id);
            if (c == null)//chua co trong GH
            {
                c = new Cart(id); //tao SPGH mới
                carts.Add(c);//add dsGh
            }
            else//dã có
            {
                c.Quantity++;//tăng số luoengj
            }
            return RedirectToAction("ListCarts");
        }

        public ActionResult ListCarts()//hiển thị giỏ hàng
        {
            List<Cart> carts = GetListCarts();

            if (carts.Count==0)//gio hang chua co Sp
            {
                return RedirectToAction("ProductView", "Product");//DSSP
            }
            ViewBag.CountProduct = Count();
            ViewBag.Total = Total();
                        
            return View(carts);
        }
        public ActionResult Delete(int id)
        {
            List<Cart> carts = GetListCarts();//lấy giỏ hàng
            Cart c = carts.Find(s => s.ProductID == id);

            if (c != null)
            {
                carts.RemoveAll(s=>s.ProductID==id);
                return RedirectToAction("ListCarts");
            }
            if (carts.Count == 0)
            {
                return RedirectToAction("ProductView", "Product");
            }
            return RedirectToAction("ListCarts");
        }

        public ActionResult OrderProduct(FormCollection fCollection)
        {

            using (TransactionScope tranScope = new TransactionScope())
            {
                try
                {
                    Order order = new Order();
                    order.OrderDate = DateTime.Now;
                    dt.Orders.InsertOnSubmit(order);
                    dt.SubmitChanges();
                    //order = dt.Orders.OrderByDescending(s => s.OrderID).Take(1).SingleOrDefault();
                    List<Cart> carts = GetListCarts();//lấy giỏ hàng
                    foreach (var item in carts)
                    {
                        Order_Detail d = new Models.Order_Detail();
                        d.OrderID = order.OrderID;
                        d.ProductID = item.ProductID;
                        d.Quantity = short.Parse(item.Quantity.ToString());
                        d.UnitPrice = item.UnitPrice;
                        d.Discount = 0;

                        dt.Order_Details.InsertOnSubmit(d);
                    }
                    dt.SubmitChanges();
                    tranScope.Complete();
                    Session["Cart"] = null;
                 }
                catch (Exception)
                {
                    tranScope.Dispose();
                    return RedirectToAction("ListCarts");
                    
                }
            }
            return RedirectToAction("OrderDetailList", "Cart");
        }

        
        public ActionResult OrderDetailList()
        {
            var p = dt.Order_Details.OrderByDescending(s=>s.OrderID).Select(s => s).ToList();
            return View(p);
        }









        

    }
}