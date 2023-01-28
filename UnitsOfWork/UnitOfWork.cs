using DAL;
using Entities;
using Entities.Files;
using Entities.Hotels;
using Entities.Transports;
using Entities.Users;
using Repositories.Generic;
using Repositories.Interface;
using UnitsOfWork.Interfaces;

namespace UnitsOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private UsageDbContext UsageContext;

        public UnitOfWork()
        {
            UsageContext = new UsageDbContext();
        }

        public UnitOfWork(UsageDbContext UsageContext)
        {
            this.UsageContext = UsageContext;
        }

        private IRepository<Image> _images;
        private IRepository<Hotel> _hotels;
        private IRepository<HotelRoom> _hotelsrooms;
        private IRepository<Tour> _tourstemplates;
        private IRepository<Transport> _transports;
        private IRepository<TransportPlace> _transportPlaces;

        private IRepository<Customer> _customers;
        private IRepository<HotelRoomReservation> _hotelsroomsreservations;
        private IRepository<Bill> _bills;
        public IRepository<Image> Images
        {
            get
            {
                if (_images == null)
                    _images = new GenericRepository<Image>(UsageContext);
                return _images;
            }
        }
        public IRepository<Hotel> Hotels
        {
            get
            {
                if (_hotels == null)
                    _hotels = new GenericRepository<Hotel>(UsageContext);
                return _hotels;
            }
        }

        public IRepository<HotelRoom> HotelsRooms
        {
            get
            {
                if (_hotelsrooms == null)
                    _hotelsrooms = new GenericRepository<HotelRoom>(UsageContext);
                return _hotelsrooms;
            }
        }
        public IRepository<Tour> ToursTemplates
        {
            get
            {
                if (_tourstemplates == null)
                    _tourstemplates = new GenericRepository<Tour>(UsageContext);
                return _tourstemplates;
            }
        }
        public IRepository<Transport> Transports
        {
            get
            {
                if (_transports == null)
                    _transports = new GenericRepository<Transport>(UsageContext);
                return _transports;
            }
        }
        public IRepository<TransportPlace> TransportPlaces
        {
            get
            {
                if (_transportPlaces == null)
                    _transportPlaces = new GenericRepository<TransportPlace>(UsageContext);
                return _transportPlaces;
            }
        }

        public IRepository<Customer> Customers
        {
            get
            {
                if (_customers == null)
                    _customers = new GenericRepository<Customer>(UsageContext);
                return _customers;
            }
        }
        public IRepository<HotelRoomReservation> HotelsRoomsReservations
        {
            get
            {
                if (_hotelsroomsreservations == null)
                    _hotelsroomsreservations = new GenericRepository<HotelRoomReservation>(UsageContext);
                return _hotelsroomsreservations;
            }
        }
        public IRepository<Bill> Bills
        {
            get
            {
                if (_bills == null)
                    _bills = new GenericRepository<Bill>(UsageContext);
                return _bills;
            }
        }

        public void DeleteDB()
        {
            UsageContext.Database.EnsureDeleted();
            UsageContext.Database.EnsureCreated();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    UsageContext.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
