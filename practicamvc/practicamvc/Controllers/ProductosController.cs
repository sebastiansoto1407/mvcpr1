﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using practicamvc.Data;
using practicamvc.Models;
using System.Text.RegularExpressions;

namespace practicamvc.Controllers
{
    public class ProductosController : Controller
    {
        private readonly ArtesaniasContext _context;

        public ProductosController(ArtesaniasContext context) => _context = context;

        private static string Normalizar(string s)
        {
            s = (s ?? string.Empty).Trim();
            s = Regex.Replace(s, @"\s{2,}", " ");
            return s;
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> CheckNombreUnico(string nombre, int id)
        {
            var n = Normalizar(nombre).ToUpperInvariant();
            var existe = await _context.Productos
                .AnyAsync(p => p.Id != id && p.Nombre.ToUpper() == n);
            return existe ? Json($"Ya existe un producto llamado “{nombre}”.") : Json(true);
        }

        public async Task<IActionResult> Index(string? q)
        {
            var query = _context.Productos.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var n = Normalizar(q);
                query = query.Where(p => p.Nombre.Contains(n) || (p.Descripcion ?? "").Contains(n));
            }

            var data = await query.OrderBy(p => p.Nombre).ToListAsync();
            ViewBag.Q = q;
            return View(data);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id is null) return NotFound();
            var prod = await _context.Productos.FirstOrDefaultAsync(m => m.Id == id);
            if (prod is null) return NotFound();
            return View(prod);
        }

        public IActionResult Create() => View(new ProductoModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductoModel producto)
        {
            producto.Nombre = Normalizar(producto.Nombre);
            producto.Descripcion = Normalizar(producto.Descripcion);

            if (await _context.Productos.AnyAsync(p => p.Nombre.ToUpper() == producto.Nombre.ToUpper()))
                ModelState.AddModelError(nameof(producto.Nombre), "Ya existe un producto con ese nombre.");

            if (!ModelState.IsValid) return View(producto);

            _context.Add(producto);
            await _context.SaveChangesAsync();
            TempData["Ok"] = "Producto creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id is null) return NotFound();
            var prod = await _context.Productos.FindAsync(id);
            if (prod is null) return NotFound();
            return View(prod);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductoModel producto)
        {
            if (id != producto.Id) return NotFound();

            producto.Nombre = Normalizar(producto.Nombre);
            producto.Descripcion = Normalizar(producto.Descripcion);

            if (await _context.Productos.AnyAsync(p => p.Id != producto.Id && p.Nombre.ToUpper() == producto.Nombre.ToUpper()))
                ModelState.AddModelError(nameof(producto.Nombre), "Ya existe otro producto con ese nombre.");

            if (!ModelState.IsValid) return View(producto);

            _context.Update(producto);
            await _context.SaveChangesAsync();
            TempData["Ok"] = "Producto actualizado.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return NotFound();
            var prod = await _context.Productos.FirstOrDefaultAsync(m => m.Id == id);
            if (prod is null) return NotFound();
            return View(prod);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prod = await _context.Productos.FindAsync(id);
            if (prod != null)
            {
                _context.Productos.Remove(prod);
                await _context.SaveChangesAsync();
                TempData["Ok"] = "Producto eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
