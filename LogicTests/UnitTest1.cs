using BLL.Services;
using DTO;
using DTO.Hotels;
using DTO.Transports;
using DTO.User;
using Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnitsOfWork;

namespace LogicTests
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public async Task AddingHotel()
        {
            var UoW = new Mock<UnitOfWork>();
            UoW.Object.DeleteDB();

            var HotelLogic = new HotelService(UoW.Object);

            await HotelLogic.AddHotelAsync(new HotelDTO("Verkhovina", 3, "Khust, Zhayvoronkova 44/2"));

            Assert.IsTrue((await HotelLogic.GetAllHotelsAsync()).Count() == 1);
            Assert.IsTrue((await HotelLogic.GetAllHotelsAsync()).ToList()[0].Name == "Verkhovina");
            Assert.IsTrue((await HotelLogic.GetAllHotelsAsync()).ToList()[0].Stars == 3);
            Assert.IsTrue((await HotelLogic.GetAllHotelsAsync()).ToList()[0].Address == "Khust, Zhayvoronkova 44/2");
        }

        [TestMethod]
        public async Task DeletingHotel()
        {
            var UoW = new Mock<UnitOfWork>();
            UoW.Object.DeleteDB();

            var HotelLogic = new HotelService(UoW.Object);

            await HotelLogic.AddHotelAsync(new HotelDTO("Verkhovina", 3, "Khust, Zhayvoronkova 44/2"));

            Assert.IsTrue((await HotelLogic.GetAllHotelsAsync()).Count() == 1);
            Assert.IsTrue((await HotelLogic.GetAllHotelsAsync()).ToList()[0].Name == "Verkhovina");
            Assert.IsTrue((await HotelLogic.GetAllHotelsAsync()).ToList()[0].Stars == 3);
            Assert.IsTrue((await HotelLogic.GetAllHotelsAsync()).ToList()[0].Address == "Khust, Zhayvoronkova 44/2");

            await HotelLogic.DeleteHotelAsync(1);

            Assert.IsTrue((await HotelLogic.GetAllHotelsAsync()).Count() == 0);
            Assert.IsTrue((await UoW.Object.HotelsRooms.GetAllAsync()).Count() == 0);


        }

        [TestMethod]
        public async Task AddingHotelRoomToHotel()
        {
            var UoW = new Mock<UnitOfWork>();
            UoW.Object.DeleteDB();

            var HotelLogic = new HotelService(UoW.Object);

            await HotelLogic.AddHotelAsync(new HotelDTO("Verkhovina", 3, "Khust, Zhayvoronkova 44/2"));

            await HotelLogic.AddHotelRoomAsync((await HotelLogic.GetAllHotelsAsync()).ToList()[0].Id, new HotelRoomDTO("STANDART", 1, 3, 250, (await HotelLogic.GetAllHotelsAsync()).ToList()[0].Id));

            Assert.IsTrue((await UoW.Object.HotelsRooms.GetAllAsync()).Count() == 1);

            Assert.IsTrue((await HotelLogic.GetAllHotelsAsync()).ToList()[0].Rooms.Count() == 1);
            Assert.IsTrue((await HotelLogic.GetAllHotelsAsync()).ToList()[0].Rooms[0].Hotel.Id == (await HotelLogic.GetAllHotelsAsync()).ToList()[0].Id);
            Assert.IsTrue((await HotelLogic.GetAllHotelsAsync()).ToList()[0].Rooms[0].Number == 1);
            Assert.IsTrue((await HotelLogic.GetAllHotelsAsync()).ToList()[0].Rooms[0].SleepingPlaces == 3);
            Assert.IsTrue((await HotelLogic.GetAllHotelsAsync()).ToList()[0].Rooms[0].Price == 250);
        }

        [TestMethod]
        public async Task ChangingTransportPlacesPrice()
        {
            var UoW = new Mock<UnitOfWork>();
            UoW.Object.DeleteDB();

            var TourLogic = new TourService(UoW.Object);

            var TransportLogic = new TransportService(UoW.Object);

            var tour = CreateTour();

            tour = TransportLogic.AddTransportToTour(tour,
                CreateTransportIn(), 30, 150,
                CreateTransportOut(), 15, 199.99m);

            await TourLogic.AddTour(tour);

            var TransportIn = await TransportLogic.GetTransportAsync(1);

            var TransportOut = await TransportLogic.GetTransportAsync(2);

            Assert.IsTrue((await UoW.Object.Transports.GetAllAsync()).Count() == 2);
            Assert.IsTrue(TransportIn.Type == "Bus");
            Assert.IsTrue(TransportIn.DeparturePoint == "Kyiv");
            Assert.IsTrue(TransportIn.DepartureTime == new DateTime(2022, 02, 25, 15, 54, 00));
            Assert.IsTrue(TransportIn.ArrivalPoint == "Hust");
            Assert.IsTrue(TransportIn.ArrivalTime == new DateTime(2022, 02, 26, 03, 22, 00));
            Assert.IsTrue(TransportIn.TransportPlaces.Count == 30);
            Assert.IsTrue(TransportIn.TransportPlaces[0].Number == 1);
            Assert.IsTrue(TransportIn.TransportPlaces[0].Price == 150);
            Assert.IsTrue(TransportIn.TransportPlaces[0].Transport.Id == TransportIn.Id);
            Assert.IsTrue(TransportOut.Type == "Bus");
            Assert.IsTrue(TransportOut.DeparturePoint == "Hust");
            Assert.IsTrue(TransportOut.DepartureTime == new DateTime(2022, 03, 01, 15, 54, 00));
            Assert.IsTrue(TransportOut.ArrivalPoint == "Kyiv");
            Assert.IsTrue(TransportOut.ArrivalTime == new DateTime(2022, 03, 02, 03, 22, 00));
            Assert.IsTrue(TransportOut.TransportPlaces.Count == 15);
            Assert.IsTrue(TransportOut.TransportPlaces[0].Number == 1);
            Assert.IsTrue(TransportOut.TransportPlaces[0].Price == 199.99m);
            Assert.IsTrue(TransportOut.TransportPlaces[0].Transport.Id == TransportOut.Id);
        }

        [TestMethod]
        public async Task DeletingTransport()
        {
            var UoW = new Mock<UnitOfWork>();
            UoW.Object.DeleteDB();

            var TourLogic = new TourService(UoW.Object);

            await TourLogic.AddTour(CreateTourWithTransportsAndHotel());

            var TransportLogic = new TransportService(UoW.Object);

            Assert.IsTrue((await UoW.Object.Transports.GetAllAsync()).Count() == 2);

            await TransportLogic.DeleteTransportAsync(1);

            Assert.IsTrue((await UoW.Object.Transports.GetAllAsync()).Count() == 1);

        }

        [TestMethod]
        public async Task AddingTour()
        {
            var UoW = new Mock<UnitOfWork>();
            UoW.Object.DeleteDB();

            var TourLogic = new TourService(UoW.Object);

            await TourLogic.AddTour(CreateTourWithTransportsAndHotel());

            var Tour = await TourLogic.GetTourAsync(1);

            Assert.IsTrue((await TourLogic.GetAllToursTemplates()).Count() == 1);
            Assert.IsTrue(Tour.Title == "Mercure Hurghada Hotel with great views");
            Assert.IsTrue(Tour.Price == 599.90m);
            Assert.IsTrue(Tour.Type == "Type");
            Assert.IsTrue(Tour.Country == "Egypt");
            Assert.IsTrue(Tour.City == "Hurghada");
            Assert.IsTrue(Tour.AviablePeopleCount == 1);
            Assert.IsTrue(Tour.Duration == 5);
            Assert.IsTrue(Tour.Description == "description");
        }

        [TestMethod]
        public async Task EditingTour()
        {
            var UoW = new Mock<UnitOfWork>();
            UoW.Object.DeleteDB();

            var TourLogic = new TourService(UoW.Object);

            await TourLogic.AddTour(CreateTour());

            var Tour = await TourLogic.GetTourAsync(1);

            Assert.IsTrue((await TourLogic.GetAllToursTemplates()).Count() == 1);
            Assert.IsTrue(Tour.Title == "Mercure Hurghada Hotel with great views");
            Assert.IsTrue(Tour.Price == 599.90m);
            Assert.IsTrue(Tour.Type == "Type");
            Assert.IsTrue(Tour.Country == "Egypt");
            Assert.IsTrue(Tour.City == "Hurghada");
            Assert.IsTrue(Tour.AviablePeopleCount == 1);
            Assert.IsTrue(Tour.Duration == 7);
            Assert.IsTrue(Tour.Description == "description");

            Tour.City = "Lviv";
            Tour.Description = "Kek";

            await TourLogic.UpdateTour(Tour.Id, Tour);

            Tour = (await TourLogic.GetAllToursTemplates()).ToList()[0];

            Assert.IsTrue((await TourLogic.GetAllToursTemplates()).Count() == 1);
            Assert.IsTrue(Tour.Title == "Mercure Hurghada Hotel with great views");
            Assert.IsTrue(Tour.Price == 599.90m);
            Assert.IsTrue(Tour.Type == "Type");
            Assert.IsTrue(Tour.Country == "Egypt");
            Assert.IsTrue(Tour.City == "Lviv");
            Assert.IsTrue(Tour.AviablePeopleCount == 1);
            Assert.IsTrue(Tour.Duration == 7);
            Assert.IsTrue(Tour.Description == "Kek");
        }

        [TestMethod]
        public async Task DeletingTour()
        {
            var UoW = new Mock<UnitOfWork>();
            UoW.Object.DeleteDB();

            var TourLogic = new TourService(UoW.Object);

            await TourLogic.AddTour(CreateTourWithTransportsAndHotel());

            Assert.IsTrue((await TourLogic.GetAllToursTemplates()).Count() == 1);

            await TourLogic.DeleteTour(1);

            Assert.IsTrue((await TourLogic.GetAllToursTemplates()).Count() == 0);
        }

        [TestMethod]
        public async Task FindingTour()
        {
            var UoW = new Mock<UnitOfWork>();
            UoW.Object.DeleteDB();

            var TourLogic = new TourService(UoW.Object);

            foreach (var tour in CreateTours())
            {
                await TourLogic.AddTour(tour);
            }

            Assert.IsTrue((await TourLogic.GetAllToursTemplates()).Count() == 5);

            var tours = await TourLogic.GetAllToursTemplates();

            Assert.IsTrue((await TourLogic.GetHotTourTemplatesAsync()).Count() == 3);
            Assert.IsTrue(TourLogic.FindTourTemplates(tours, "tour").Any());
            Assert.IsTrue(!TourLogic.FindTourTemplates(tours, "Error").Any());
            Assert.IsTrue(TourLogic.FindTourTemplates(tours, "Mercure").ToList()[0].City == "Hurghada");

            Assert.IsTrue(TourLogic.FindTourTemplates(tours, "Sharm El Sheikh").Count() == 1);
            Assert.IsTrue(TourLogic.FindTourTemplates(tours, "Egypt").Count() == 2);
            Assert.IsTrue(TourLogic.FindTourTemplates(tours, "Turkey").ToList()[0].City == "Antalya");

            Assert.IsTrue(TourLogic.FindTourTemplates(tours, "Italy").Count() == 1);
            Assert.IsTrue(TourLogic.FindTourTemplates(tours, "Pisa").Count() == 1);

            Assert.IsTrue(TourLogic.FindTourTemplates(tours, "Aqua Blu Resort").ToList()[0].Country == "Egypt");

            Assert.IsTrue(TourLogic.FindTourTemplatesByDuration(tours, 0, 10).Count() == 5);
            Assert.IsTrue(TourLogic.FindTourTemplatesByDuration(tours, 10, 10).Count() == 1);
            Assert.IsTrue(TourLogic.FindTourTemplatesByDuration(tours, 1000000, 10000000).Count() == 0);
            Assert.IsTrue(TourLogic.FindTourTemplatesByDuration(tours, 9, 12).ToList()[0].Country == "Germany");
            Assert.IsTrue(TourLogic.FindTourTemplatesByDuration(tours, 10, 15).ToList()[0].Price == 950.59m);


            Assert.IsTrue(TourLogic.FindTourTemplatesByPrice(tours, 1800m, 2500m).Count() == 1);
            Assert.IsTrue(TourLogic.FindTourTemplatesByPrice(tours, 1400m, 2000m).Count() == 2);
            Assert.IsTrue(TourLogic.FindTourTemplatesByPrice(tours, 100m, 400m).Count() == 0);
        }

        [TestMethod]
        public async Task ReserveTour()
        {
            var UoW = new Mock<UnitOfWork>();
            UoW.Object.DeleteDB();

            var user = new CustomerDTO("Niko", "Tym", "1@gmail.com", "NikoWASD");

            var UserLogic = new UserService(UoW.Object);
            var HotelLogic = new HotelService(UoW.Object);
            var TourLogic = new TourService(UoW.Object);

            await UserLogic.AddUserAsync(user);

            await TourLogic.AddTour(CreateTourWithTransportsAndHotel());

            await HotelLogic.AddHotelRoomAsync(
                (await HotelLogic.GetAllHotelsAsync()).ToList()[0].Id,
                new HotelRoomDTO("ULTRACLASS", 1, 3, 250,
                (await HotelLogic.GetAllHotelsAsync()).ToList()[0].Id));


            Assert.IsTrue((await UoW.Object.HotelsRooms.GetAllAsync()).Count() == 1);

            Assert.IsTrue((await HotelLogic.GetAllHotelsAsync()).ToList()[0].Rooms.Count() == 1);

            await UserLogic.BuildBillAsync(1,
                null, 0, 1,
                "ULTRACLASS",
                new DateTime(2018, 12, 21),
                new DateTime(2018, 12, 25));

            var Bills = await UserLogic.GetBillsAsync(1);

            Assert.IsTrue(Bills[0].RoomReservation.HotelAddress == "Some Address");
            Assert.IsTrue(Bills[0].RoomReservation.HotelName == "Mercure Hurghada Hotel");
            Assert.IsTrue(Bills[0].RoomReservation.HotelRoomNumber == 1);
            Assert.IsTrue(Bills[0].RoomReservation.HotelRoomPrice == 250);
            Assert.IsTrue(Bills[0].RoomReservation.HotelRoomSleepingPlaces == 3);
            Assert.IsTrue(Bills[0].RoomReservation.HotelStars == 3);
            Assert.IsTrue(Bills[0].RoomReservation.ClientName == "Niko");
            Assert.IsTrue(Bills[0].RoomReservation.ClientSurname == "Tym");
            Assert.IsTrue(Bills[0].RoomReservation.ArrivalDate.CompareTo(new DateTime(2018, 12, 21)) == 0);
            Assert.IsTrue(Bills[0].RoomReservation.DepartureDate.CompareTo(new DateTime(2018, 12, 25)) == 0);

            var HotelRoom = (await UoW.Object.HotelsRooms.GetAllAsync()).FirstOrDefault();

            Assert.IsTrue(HotelRoom.BookedDays.Where(d => d.Time == new DateTime(2018, 12, 21)).Any());
            Assert.IsTrue(HotelRoom.BookedDays.Count() == 4);

            await Assert.ThrowsExceptionAsync<AlreadyBookedItemException>(async delegate
            {
                await UserLogic.BuildBillAsync(1,
                    new(), 0, 1,
                    "ULTRACLASS",
					new DateTime(2018, 12, 21),
					new DateTime(2018, 12, 25));
            });

            await UserLogic.BuildBillAsync(1,
                     new(), 0, 1,
                     "ULTRACLASS",
					 new DateTime(2018, 12, 25),
					 new DateTime(2018, 12, 28));

            Bills = await UserLogic.GetBillsAsync(1);

            Assert.IsTrue(Bills.Count == 2);
            Assert.IsTrue(Bills[1].RoomReservation.HotelAddress == "Some Address");
            Assert.IsTrue(Bills[1].RoomReservation.HotelName == "Mercure Hurghada Hotel");
            Assert.IsTrue(Bills[1].RoomReservation.HotelRoomNumber == 1);
            Assert.IsTrue(Bills[1].RoomReservation.HotelRoomPrice == 250);
            Assert.IsTrue(Bills[1].RoomReservation.HotelRoomSleepingPlaces == 3);
            Assert.IsTrue(Bills[1].RoomReservation.HotelStars == 3);
            Assert.IsTrue(Bills[1].RoomReservation.ClientName == "Niko");
            Assert.IsTrue(Bills[1].RoomReservation.ClientSurname == "Tym");
            Assert.IsTrue(Bills[1].RoomReservation.ArrivalDate.CompareTo(new DateTime(2018, 12, 25)) == 0);
            Assert.IsTrue(Bills[1].RoomReservation.DepartureDate.CompareTo(new DateTime(2018, 12, 28)) == 0);

            HotelRoom = (await UoW.Object.HotelsRooms.GetAllAsync()).FirstOrDefault();

            Assert.IsTrue(HotelRoom.BookedDays.Where(d => d.Time == new DateTime(2018, 12, 25)).Any());
            Assert.IsTrue(HotelRoom.BookedDays.Count() == 7);



            await Assert.ThrowsExceptionAsync<AlreadyBookedItemException>(async delegate
            {
                await UserLogic.BuildBillAsync((await UserLogic.GetUserAsync(1)).Id,
                    new(), 0, 1,
                    "ULTRACLASS",
					new DateTime(2018, 12, 26),
					new DateTime(2018, 12, 28));
            });
        }

        private TourDTO CreateTour()
        {
            var hotel1 = new HotelDTO()
            {
                Name = "Mercure Hurghada Hotel",
                Address = "Some Address",
                Stars = 3,
            };
            return new()
            {
                Title = "Mercure Hurghada Hotel with great views",
                Type = "Type",
                Country = "Egypt",
                City = "Hurghada",
                Hotel = hotel1,
                Meal = "All inclusive",
                Duration = 7,
                AviablePeopleCount = 1,
                IsHotOffer = true,
                Price = 599.90m,
                Description = "description",
            };
        }
        private TourDTO CreateTourWithTransportsAndHotel()
        {
            var transport1 = new TransportDTO()
            {
                Type = "Bus",
                DeparturePoint = "Ukraine, Kiev",
                ArrivalPoint = "Egypt, Hurghada",
                DepartureTime = new DateTime(2022, 02, 24, 15, 54, 00),
                ArrivalTime = new DateTime(2022, 02, 25, 03, 22, 00),
                TransportPlaces = new()
                {
                    new TransportPlaceDTO()
                    {
                        Number = 1,
                        Price = 64.09M
                    },
                    new TransportPlaceDTO()
                    {
                        Number = 2,
                        Price = 64.09M
                    },
                    new TransportPlaceDTO()
                    {
                        Number = 3,
                        Price = 64.09M
                    },
                    new TransportPlaceDTO()
                    {
                        Number = 4,
                        Price = 64.09M
                    },
                    new TransportPlaceDTO()
                    {
                        Number = 5,
                        Price = 64.09M
                    },
                    new TransportPlaceDTO()
                    {
                        Number = 6,
                        Price = 64.09M
                    },
                }
            }; var transport2 = new TransportDTO()
            {
                Type = "Bus",
                DeparturePoint = "Egypt, Hurghada",
                ArrivalPoint = "Ukraine, Kiev",
                DepartureTime = new DateTime(2022, 03, 01, 15, 54, 00),
                ArrivalTime = new DateTime(2022, 03, 02, 03, 22, 00),
                TransportPlaces = new()
                {
                    new TransportPlaceDTO()
                    {
                        Number = 1,
                        Price = 64.09M
                    },
                    new TransportPlaceDTO()
                    {
                        Number = 2,
                        Price = 64.09M
                    },
                    new TransportPlaceDTO()
                    {
                        Number = 3,
                        Price = 64.09M
                    },
                    new TransportPlaceDTO()
                    {
                        Number = 4,
                        Price = 64.09M
                    },
                    new TransportPlaceDTO()
                    {
                        Number = 5,
                        Price = 64.09M
                    },
                    new TransportPlaceDTO()
                    {
                        Number = 6,
                        Price = 64.09M
                    },
                }
            };
            var hotel1 = new HotelDTO()
            {
                Name = "Mercure Hurghada Hotel",
                Address = "Some Address",
                Stars = 3,
            };
            return new()
            {
                Title = "Mercure Hurghada Hotel with great views",
                Type = "Type",
                Country = "Egypt",
                City = "Hurghada",
                Hotel = hotel1,
                Meal = "All inclusive",
                Duration = 5,
                AviablePeopleCount = 1,
                TransportIn = transport1,
                TransportOut = transport2,
                IsHotOffer = true,
                Price = 599.90m,
                Description = "description",
            };
        }
        private List<TourDTO> CreateTours()
        {
            var hotel1 = new HotelDTO()
            {
                Name = "Mercure Hurghada Hotel",
                Address = "Some Address",
                Stars = 2,
            };
            var hotel2 = new HotelDTO()
            {
                Name = "Albatros Aqua Blu Resort",
                Address = "Some Address",
                Stars = 4,
            };
            var hotel3 = new HotelDTO()
            {
                Name = "hotel3",
                Address = "Some Address",
                Stars = 1,
            };
            var hotel4 = new HotelDTO()
            {
                Name = "hotel4",
                Address = "Some Address",
                Stars = 4,
            };
            var hotel5 = new HotelDTO()
            {
                Name = "hotel5",
                Address = "Some Address",
                Stars = 7,
            };
            return new()
            {
                new TourDTO
                {
                    Title = "Mercure Hurghada Hotel with great views",
                    Type = "Vacation with children",
                    Country = "Egypt",
                    City = "Hurghada",
                    Meal = "All inclusive",
                    AviablePeopleCount = 4,
                    Hotel = hotel1,
                    IsHotOffer = true,
                    Price = 599.90m,
                    Duration = 5,
                    Description = "desciption",
                },

                new TourDTO
                {
                    Title = "Albatros Aqua Blu Resort Sharm El Sheikh",
                    Type = "Beach tour",
                    Country = "Egypt",
                    City = "Sharm El Sheikh",
                    Meal = "Without meal",
                    AviablePeopleCount = 1,
                    Hotel = hotel2,
                    Price = 2000m,
                    Duration = 7,
                    Description = "desciption",
                },

                new TourDTO
                {
                    Title = "Tour",
                    Type = "Family vacation",
                    Country = "Germany",
                    City = "Dusseldorf",
                    Meal = "All inclusive",
                    AviablePeopleCount = 3,
                    Hotel = hotel3,
                    IsHotOffer = true,
                    Price = 950.59m,
                    Duration = 10,
                    Description = "desciption",
                },

                new TourDTO
                {
                    Title = "Another tour",
                    Type = "Vacation with children",
                    Country = "Italy",
                    City = "Pisa",
                    Meal = "WithoutMeal",
                    AviablePeopleCount = 4,
                    Hotel = hotel4,
                    IsHotOffer = true,
                    Price = 1300m,
                    Duration = 7,
                    Description = "desciption",
                },
                new TourDTO
                {
                    Title = "Some tour",
                    Type = "Beach tour",
                    Country = "Turkey",
                    City = "Antalya",
                    Meal = "All inclusive",
                    AviablePeopleCount = 2,
                    Hotel = hotel5,
                    Price = 1500m,
                    Duration = 5,
                    Description = "description",
                }
            };
        }

        private TransportDTO CreateTransportIn()
        {
            return new TransportDTO("Bus",
                "Kyiv",
                new DateTime(2022, 02, 25, 15, 54, 00),
                "Hust",
                new DateTime(2022, 02, 26, 03, 22, 00));
        }
        private TransportDTO CreateTransportOut()
        {
            return new TransportDTO("Bus",
                "Hust",
                new DateTime(2022, 03, 01, 15, 54, 00),
                "Kyiv",
                new DateTime(2022, 03, 02, 03, 22, 00));
        }
    }
}