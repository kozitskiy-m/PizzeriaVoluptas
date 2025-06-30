using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PizzeriaVoluptas.Models.Db;

namespace PizzeriaVoluptas.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "admin")]
    public class DishesController : Controller
    {
        private readonly PizzaVoluptasContext _context;

        public DishesController(PizzaVoluptasContext context)
        {
            _context = context;
        }

        // GET: Admin/Dishes
        public async Task<IActionResult> Index()
        {
            return View(await _context.Dishes.ToListAsync());
        }

        public IActionResult DeleteGallery(int id)
        {
            var gallery = _context.DishGaleries.FirstOrDefault(x => x.Id == id);
            if (gallery == null)
            {
                return NotFound();
            }
            string d = Directory.GetCurrentDirectory();
            string fn = d + "\\wwwroot\\images\\banners\\" + gallery.DishId;

            if (System.IO.File.Exists(fn))
            {
                System.IO.File.Delete(fn);
            }
            _context.Remove(gallery);
            _context.SaveChanges();

            return Redirect("edit/" + gallery.DishId);
        }


        // GET: Admin/Dishes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dish = await _context.Dishes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dish == null)
            {
                return NotFound();
            }

            return View(dish);
        }

        // GET: Admin/Dishes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Dishes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,FullDesc,Price,Discount,ImageName,Qty,Ingredients")] Dish dish, IFormFile? MainImage, IFormFile[]? GalleryImages)
        {
            if (ModelState.IsValid)
            {
                //=======saving main image========

                if (MainImage != null)
                {
                    dish.ImageName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(MainImage.FileName);
                    string fn;
                    fn = Directory.GetCurrentDirectory();
                    string ImagePath = fn + "\\wwwroot\\images\\banners\\" + dish.ImageName;

                    using (var stream = new FileStream(ImagePath, FileMode.Create))
                    {
                        MainImage.CopyTo(stream);
                    }
                }

                //================================
                                
                _context.Add(dish);
                await _context.SaveChangesAsync();

                //=======saving gallery images========

                if (GalleryImages != null)
                {
                    foreach(var item in GalleryImages)
                    {
                        var newgallery = new DishGalery();
                        newgallery.DishId = dish.Id;
                        //-------------------------
                        newgallery.ImageName = Guid.NewGuid().ToString() + System.IO.Path.GetExtension(item.FileName);
                        string fn;
                        fn = Directory.GetCurrentDirectory();
                        string ImagePath = fn + "\\wwwroot\\images\\banners\\" + newgallery.ImageName;

                        using (var stream = new FileStream(ImagePath, FileMode.Create))
                        {
                            item.CopyTo(stream);
                        }
                        //-------------------------
                        _context.DishGaleries.Add(newgallery);
                    }
                }
                await _context.SaveChangesAsync();
                //====================================

                return RedirectToAction(nameof(Index));
            }
            return View(dish);
        }

        // GET: Admin/Dishes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dish = await _context.Dishes.FindAsync(id);
            if (dish == null)
            {
                return NotFound();
            }

            ViewData["gallery"] = _context.DishGaleries.Where(x=>x.DishId == dish.Id).ToList();

            return View(dish);
        }

        // POST: Admin/Dishes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,FullDesc,Price,Discount,ImageName,Qty,Ingredients")] Dish dish, IFormFile? MainImage, IFormFile[]? GalleryImages)
        {
            if (id != dish.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //============save image============

                    if (MainImage != null)
                    {
                        string d = Directory.GetCurrentDirectory();
                        string fn = d + "\\wwwroot\\images\\banners\\" + dish.ImageName;
                        //------------------------------------------------
                        if (System.IO.File.Exists(fn))
                        {
                            System.IO.File.Delete(fn);
                        }
                        //------------------------------------------------
                        using (var stream = new FileStream(fn, FileMode.Create))
                        {
                            MainImage.CopyTo(stream);
                        }
                        //------------------------------------------------
                    }
                    //========================================================
                    if (GalleryImages != null)
                    {
                        foreach (var item in GalleryImages)
                        {

                            var imageName = Guid.NewGuid() + Path.GetExtension(item.FileName);
                            //------------------------------------------------
                            string d = Directory.GetCurrentDirectory();
                            string fn = d + "\\wwwroot\\images\\banners\\" + imageName;
                            //------------------------------------------------
                            using (var stream = new FileStream(fn, FileMode.Create))
                            {
                                item.CopyTo(stream);
                            }
                            //------------------------------------------------
                            var galleryItem = new DishGalery();
                            galleryItem.ImageName = imageName;
                            galleryItem.DishId = dish.Id;
                            //------------------------------------------------
                            _context.DishGaleries.Add(galleryItem);
                        }
                    }


                    //==================================
                    _context.Update(dish);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DishExists(dish.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(dish);
        }

        // GET: Admin/Dishes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dish = await _context.Dishes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dish == null)
            {
                return NotFound();
            }

            return View(dish);
        }

        // POST: Admin/Dishes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dish = await _context.Dishes.FindAsync(id);
            if (dish != null)
            {

                //=========delete images==================

                string d = Directory.GetCurrentDirectory();
                string fn = d + "\\wwwroot\\images\\banners\\";
                //----------
                string mainImagePath = fn + dish.ImageName;
                //--------------------------
                if (System.IO.File.Exists(mainImagePath))
                {
                    System.IO.File.Delete(mainImagePath);
                }
                //--------------------------
                var galleries = _context.DishGaleries.Where(x => x.DishId == id).ToList();
                if (galleries != null)
                {
                    //--------------------------
                    foreach (var item in galleries)
                    {
                        string galleryImagePath = fn + item.ImageName;
                        //--------------------------
                        if (System.IO.File.Exists(galleryImagePath))
                        {
                            System.IO.File.Delete(galleryImagePath);
                        }
                        //--------------------------
                    }
                    //--------------------------
                    _context.DishGaleries.RemoveRange(galleries);
                }

                //========================================

                _context.Dishes.Remove(dish);

                
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DishExists(int id)
        {
            return _context.Dishes.Any(e => e.Id == id);
        }
    }
}
