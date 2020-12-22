using Geo.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace Geo.Data
{
    public class UnitOfWork<T> : IDisposable
        where T : class
    {
        private GeoDbContext _context;
        private GenericRepository<T> Repository;
        public UnitOfWork(IConfiguration configuration, IHttpContextAccessor accessor)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GeoDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("GeoConnection"));
            _context = new GeoDbContext(optionsBuilder.Options, accessor, configuration);
        }

        public GenericRepository<T> Generic
        {
            get
            {
                if (Repository == null)
                    Repository = new GenericRepository<T>(_context);
                return Repository;
            }
        }

        public void Save() => _context.SaveChanges();
        public void Update(T model) => _context.Entry(model).State = EntityState.Modified;

        public void Delete(int Id)
        {
            var model = _context.Set<T>().Find(Id);
            if (model != null)
                _context.Set<T>().Remove(model);
        }

        public void Delete(IEnumerable<T> model)
        {
            if (model != null)
                _context.Set<T>().RemoveRange(model);
        }

        public void Delete(int UserId, int RoleId)
        {
            var model = _context.Set<T>().Find(UserId, RoleId);
            if (model != null)
                _context.Set<T>().Remove(model);
        }

        public void Delete(string Id)
        {
            var model = _context.Set<T>().Find(Id);
            if (model != null)
                _context.Set<T>().Remove(model);
        }
        /***********************/

        public byte[] IFormFileToByte(IFormFile byteFile)
        {
            if (byteFile != null)
            {
                using (var binaryRead = new BinaryReader(byteFile.OpenReadStream()))
                {
                    return binaryRead.ReadBytes(Convert.ToInt32(byteFile.Length));
                }
            }
            return null;
        }

        /**************************/

        private bool disposed = false;
        public virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                this.disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
