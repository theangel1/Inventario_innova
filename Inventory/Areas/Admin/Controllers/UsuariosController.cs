using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Inventory.Data;
using Inventory.Extensions;
using Inventory.Models;
using Inventory.Models.ViewModel;
using Inventory.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Inventory.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Admin)]
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        [BindProperty]
        public AppUserViewModel UserVM { get; set; }

        public UsuariosController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;

            UserVM = new AppUserViewModel()
            {
                User = new ApplicationUser(),
            };
        }

        /*== INDEX ==*/
        #region Index Views

        //== Index - Get
        public IActionResult Index()
        {
            return View();
        }

        public async Task<JsonResult> GetUsers()
        {
            try
            {
                var data = new List<JObject>();
                var enUser = await _db.ApplicationUser.Include(i => i.Empresa).ToListAsync();
                var _rolId = await _db.UserRoles.ToListAsync(); 
                var _roleUser = await _db.Roles.ToListAsync(); 

                foreach (var item in enUser)
                {
                    var rolId = _rolId.FirstOrDefault(i => i.UserId == item.Id);
                    var rolName = _roleUser.FirstOrDefault(i => i.Id == rolId.RoleId);

                    JObject objWS = new JObject
                    {
                        { "id"     , item.Id      },
                        { "email"  , item.Email   },
                        { "nombre" , item.Nombre  },
                        { "rol"    , rolName.Name },
                        { "empresa", item.Empresa.Nombre },
                    };

                    data.Add(objWS);
                }
                var totalRecords = data.Count();
                return Json(new { recordsFiltered = totalRecords, data });
            }
            catch (Exception ex)
            {
                return Json(ex);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(string id)
        {
            var UserFromDb = await _db.ApplicationUser.FindAsync(id);

            if (UserFromDb.LockoutEnabled == true)
            {
                UserFromDb.LockoutEnabled = false;
            }
            else
            {
                UserFromDb.LockoutEnabled = true;
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        /*== EDIT ==*/
        #region Edit Views

        //== Get - Edit

        public async Task<IActionResult> Edit(string id)
        {
            var _rolId = _db.UserRoles.First(m => m.UserId == id); // Trae el rol Id del usuario
            var _roles = _db.Roles.First(m => m.Id == _rolId.RoleId); // Trae el objeto rol del usuario
            var _rolesOb = _db.Roles.ToList(); // Trae una lista de todos los roles

            AppUserViewModel UserRolVM = new AppUserViewModel()
            {
                User = await _db.ApplicationUser.FindAsync(id),
                Rol = _roles.NormalizedName,
                RolId = _roles.Id,
                RolList = _rolesOb,
                EmpresaList = _db.Empresa.ToList(),
            };
            return View(UserRolVM);
        }

        //== Post - Edit 
        //  [Authorize(Roles = SD.Tier3)]
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var UserFromDb = await _userManager.FindByEmailAsync(model.User.Email);
                var AppUserFromDb = await _db.ApplicationUser.FindAsync(UserFromDb.Id);
                var _roles = _db.Roles.First(m => m.Id == model.RolId);

                AppUserFromDb.Nombre = model.User.Nombre;
                AppUserFromDb.EmpresaId = UserVM.User.EmpresaId;

                //== Elimina todos los roles del usuario
                var _roleDel = await _userManager.GetRolesAsync(UserFromDb);
                await _userManager.RemoveFromRolesAsync(UserFromDb, _roleDel.ToArray());

                //== Asigna nuevo rol
                await _userManager.AddToRoleAsync(UserFromDb, _roles.Name);

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                AppUserViewModel UserRolVM = new AppUserViewModel()
                {
                    User = await _db.ApplicationUser.FindAsync(model.User.Id),
                    Rol = model.Rol,
                    RolId = model.RolId,
                    RolList = _db.Roles.ToList(),
                };
                return View(UserRolVM);
            }
        }
        #endregion


        #region Delete Views

        //== Get - Delete
        // [Authorize(Roles = SD.Tier3)]
        public async Task<IActionResult> Delete(string id)
        {
            var _rolId = _db.UserRoles.First(m => m.UserId == id);
            var _roles = _db.Roles.First(m => m.Id == _rolId.RoleId);


            AppUserViewModel UserRolVM = new AppUserViewModel()
            {
                User = await _db.ApplicationUser.FindAsync(id),
                Rol = _roles.NormalizedName
            };

            return View(UserRolVM);
        }

        //== Post - Delete
        // [Authorize(Roles = SD.Tier3)]
        [HttpPost, ValidateAntiForgeryToken, ActionName("Delete")]
        public async Task<IActionResult> DeletePOST(string id)
        {
            var UserFromDb = await _db.ApplicationUser.FindAsync(id);

            if (UserFromDb == null)
            {
                return NotFound();
            }

            _db.Remove(UserFromDb);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        #endregion


        #region Create Views        

        public IActionResult Create()
        {
            AppUserViewModel UserVM = new AppUserViewModel()
            {
                EmpresaList = _db.Empresa.ToList(),
                RolList = _db.Roles.ToList()
            };
            return View(UserVM);
        }

        //== Create - Post        
        [HttpPost, ActionName("Create")]
        public async Task<IActionResult> CreatePost()
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = UserVM.User.Email,
                    Email = UserVM.User.Email,
                    Nombre = UserVM.User.Nombre,
                    EmpresaId = UserVM.User.EmpresaId,
                    PhoneNumber = UserVM.User.PhoneNumber
                };
                var result = await _userManager.CreateAsync(user, UserVM.Pass);

                if (result.Succeeded)
                {
                    //== Encuentra el Rol y lo asigna 
                    var _role = await _db.Roles.FindAsync(UserVM.RolId);
                    string _roleString = _role.Name;
                    await _userManager.AddToRoleAsync(user, _roleString);

                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    AppUserViewModel UserRolVM = new AppUserViewModel()
                    {
                        User = UserVM.User,
                        Rol = UserVM.Rol,
                        RolId = UserVM.RolId,
                        RolList = _db.Roles.ToList(),
                        EmpresaList = _db.Empresa.ToList(),
                    };

                    var resultado = result.Errors.ToList();
                    string bag = string.Empty ;
                    foreach (var item in resultado)
                    {
                        bag = item.Description;
                    }

                    
                    ViewBag.serverError = bag;
                  
                    
                    return View(UserRolVM);
                }
            }
            else
            {
                AppUserViewModel UserRolVM = new AppUserViewModel()
                {
                    User = UserVM.User,
                    Rol = UserVM.Rol,
                    RolId = UserVM.RolId,
                    RolList = _db.Roles.ToList(),
                    EmpresaList = _db.Empresa.ToList(),
                };

                return View(UserRolVM);
            }
        }

        #endregion
    }
}