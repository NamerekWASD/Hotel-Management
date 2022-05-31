using BLL.Interfaces;
using BLL.Ninject;
using DTO;
using DTO.Hotels;
using DTO.Transports;
using DTO.User;
using Entities;
using Entities.Hotels;
using Entities.Hotels.Times;
using Entities.Transports;
using Entities.Users;
using Exceptions;
using UnitsOfWork.Interfaces;

namespace BLL.Services
{
    public class UserService : IUserService
    {

        IUnitOfWork UoW;

        public UserService(IUnitOfWork UoW)
        {
            this.UoW = UoW;
        }

        public UserService()
        {
            UoW = DependencyResolver.ResolveUoW();
        }

        public async Task AddUserAsync(CustomerDTO NewUser)
        {
            await UoW.Customers
                .AddAsync(Tools.Mapper.
                Map<CustomerDTO, Customer>(NewUser));
        }

        public async Task<IEnumerable<CustomerDTO>> GetAllUsersAsync()
        {
            return Tools.Mapper
                .Map<IEnumerable<Customer>, List<CustomerDTO>>(await UoW.
                Customers.GetAllAsync());
        }

        public async Task<CustomerDTO> GetUserAsync(int Id)
        {
            return Tools.Mapper
                .Map<Customer, CustomerDTO>(await UoW.
                Customers.GetAsync(Id));
        }

        public async Task EditUserAsync(int Id,
            CustomerDTO User)
        {
            await UoW.Customers.ModifyAsync(Id, Tools.Mapper.Map<CustomerDTO, Customer>(User));
        }

        public async Task DeleteUserAsync(int Id)
        {
            await UoW.Customers.DeleteAsync(Id);
        }
        public async Task<BillDTO> BuildBillAsync(CustomerDTO customer,
            List<CustomerDTO> AdditionalTourists,
            decimal DepositAmount,
            int TourId,
            string HotelRoomName,
            DateTimeOffset ArrivalDate,
            DateTimeOffset DepartureDate)
        {

            var tourists = new List<CustomerDTO>();

            if (AdditionalTourists != null && AdditionalTourists.Any())
            {
                foreach (var item in AdditionalTourists)
                {
                    tourists.Add(item);
                }
            }

            BillDTO bill = new()
            {
                CustomerWhoBook = customer,
                Tourists = tourists,
                DepositAmount = DepositAmount,
                Created = DateTime.Now,
                Status = "Sent",
            };

            var Tour = Tools.Mapper.Map<TourDTO>(await UoW.ToursTemplates.GetAsync(TourId));

            bill = ReserveTour(bill, Tour);
            bill = await ReserveRoomAsync(bill, customer, HotelRoomName, ArrivalDate, DepartureDate);
            bill = await ReserveTicketAsync(bill, AdditionalTourists, Tour.TransportIn.Id, Tour.TransportOut.Id);

            await UoW.Bills.AddAsync(Tools.Mapper.Map<Bill>(bill));

            return bill;
        }
        public async Task<BillDTO> BuildBillAsync(int CustomerWhoBookId,
            List<CustomerDTO> AdditionalTourists,
            decimal DepositAmount,
            int TourId,
            string HotelRoomName,
            DateTimeOffset ArrivalDate,
            DateTimeOffset DepartureDate)
        {

            var tourists = new List<CustomerDTO>();

            if (AdditionalTourists != null && AdditionalTourists.Any())
            {
                foreach (var item in AdditionalTourists)
                {
                    tourists.Add(item);
                }
            }

            BillDTO bill = new()
            {
                CustomerWhoBookId = CustomerWhoBookId,
                Tourists = tourists,
                DepositAmount = DepositAmount,
                Created = DateTime.Now,
                Status = "Sent",
            };

            var Tour = Tools.Mapper.Map<TourDTO>(await UoW.ToursTemplates.GetAsync(TourId));

            bill = ReserveTour(bill, Tour);
            bill = await ReserveRoomAsync(bill, await GetUserAsync(CustomerWhoBookId), HotelRoomName, ArrivalDate, DepartureDate);
            bill = await ReserveTicketAsync(bill, AdditionalTourists, Tour.TransportIn.Id, Tour.TransportOut.Id);

            await UoW.Bills.AddAsync(Tools.Mapper.Map<Bill>(bill));

            return bill;
        }

        private BillDTO ReserveTour(BillDTO bill,
            TourDTO Tour)
        {
            bill.TourId = Tour.Id;
            return bill;
        }

        private async Task<BillDTO> ReserveRoomAsync(BillDTO bill,
            CustomerDTO Customer,
            string HotelRoomName,
            DateTimeOffset ArrivalDate,
            DateTimeOffset DepartureDate)
        {
            var hotelroom = (await UoW.HotelsRooms
                .GetAllAsync()).FirstOrDefault(r =>
                r.Name.ToUpper() == HotelRoomName.ToUpper());



            foreach (var d in hotelroom.BookedDays)
            {
                DateTimeOffset FakeArrival = ArrivalDate;
                DateTimeOffset FakeDeparture = DepartureDate;
                while (FakeArrival.CompareTo(FakeDeparture) < 0)
                {
                    if (d.Time.Date.CompareTo(FakeArrival.Date) == 0)
                        throw new AlreadyBookedItemException($"Rooms \"{HotelRoomName}\" " +
                            $"are not aviable for {d.Time.Day}.{d.Time.Month}.{d.Time.Year}");
                    
                    FakeArrival = FakeArrival.AddDays(1);
                }
            }

            var reserv = new HotelRoomReservation(hotelroom,
                Customer.Firstname,
                Customer.Lastname,
                ArrivalDate.Date,
                DepartureDate.Date);

            while (ArrivalDate.CompareTo(DepartureDate) < 0)
            {
                hotelroom.BookedDays.Add(new DTOffset { Time = ArrivalDate.Date });
                ArrivalDate = ArrivalDate.AddDays(1);
            }

            await UoW.HotelsRooms.ModifyAsync(hotelroom.Id, hotelroom);

            bill.RoomReservation = Tools.Mapper.Map<HotelRoomReservationDTO>(reserv);

            return bill;
        }

        private async Task<BillDTO> ReserveTicketAsync(BillDTO bill,
            List<CustomerDTO> tourists,
            int TransportInId,
            int TransportOutId)
        {

            if (TransportInId != 0)
            {
                Transport transportIn = await UoW.Transports
                    .GetAsync(TransportInId);

                bill = await AddTicketAsync(bill,
                    transportIn,
                    Tools.Mapper.Map<List<Customer>>(tourists));
            }
            if (TransportOutId != 0)
            {
                Transport transportOut = await UoW.Transports
                    .GetAsync(TransportOutId);

                bill = await AddTicketAsync(bill,
                    transportOut,
                    Tools.Mapper.Map<List<Customer>>(tourists));
            }

            return bill;
        }
        private async Task<BillDTO> AddTicketAsync(BillDTO bill,
            Transport transport,
            List<Customer> tourists)
        {

            var FreeTransportPlaces =
                transport.TransportPlaces
                .FindAll(p =>
                !p.IsBooked);

            if (FreeTransportPlaces.Count < tourists.Count)
            {
                throw new AlreadyBookedItemException($"For this transport {transport.Id} aviable only {FreeTransportPlaces} seat(-s)!");
            }
            else
            {
                for (int i = 0; i < tourists.Count; i++)
                {
                    Customer? tourist = tourists[i];

                    FreeTransportPlaces[i].IsBooked = true;

                    await UoW.TransportPlaces.ModifyAsync(FreeTransportPlaces[i].Id, FreeTransportPlaces[i]);

                    bill.Tickets.Add(Tools.Mapper
                        .Map<TransportTicketDTO>(
                        new TransportTicket(FreeTransportPlaces[i],
                        tourist.Firstname,
                        tourist.Lastname)));
                }

                return bill;
            }
        }
        public async Task<List<BillDTO>> GetBillsAsync(int CustomerId)
        {
            return Tools.Mapper.Map<List<BillDTO>>(await UoW.Bills.GetAllAsync(b => b.CustomerWhoBookId == CustomerId));
        }
    }
}
