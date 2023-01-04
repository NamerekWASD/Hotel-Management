using BLL.Interfaces;
using DTO;
using DTO.Hotels;
using DTO.Transports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Hotels;
using Models.Transports;
using WebApp.Models;
using WebApp.Ninject;

namespace WebApp.Controllers
{
    [Authorize(Roles = "Administrator, Manager")]
    public class ManagerController : Controller
    {
        private readonly ITourService _tourService;
        private readonly IHotelService _hotelService;
        private readonly ITransportService _transportService;
        private IWebHostEnvironment _appEnvironment;
        public ManagerController(IWebHostEnvironment appEnvironment)
        {
            _tourService = UIDependencyResolver<ITourService>.ResolveDependency();
            _hotelService = UIDependencyResolver<IHotelService>.ResolveDependency();
            _transportService = UIDependencyResolver<ITransportService>.ResolveDependency();
            _appEnvironment = appEnvironment;
        }
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ManageTours(string SearchString)
        {
            var tours = await _tourService.
                GetAllToursTemplates();
            if (!string.IsNullOrEmpty(SearchString))
            {
                tours = _tourService.FindTourTemplates(tours, SearchString);
            }
            var manageToursVM = new ManageToursVM()
            {
                Tours = Tools.Mapper
                .Map<IEnumerable<TourDTO>, IEnumerable<TourModel>>(tours).ToList(),
            };
            return View(manageToursVM);
        }

        public ActionResult CreateTour()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTour(TourVM tourModel)
        {
            var tour = tourModel.Tour;

            tour.Price = int.Parse(tourModel.TourPrice);

            tour.Hotel = tourModel.Hotel;

            tour = Tools.Mapper
                .Map<TourDTO, TourModel>(_transportService
                .AddTransportToTour(Tools.Mapper.Map<TourModel, TourDTO>(tourModel.Tour),
                Tools.Mapper.Map<TransportModel, TransportDTO>(tourModel.TransportIn),
                tourModel.AvailibleSeatsOut,
                tourModel.PriceForTicketIn.ToDecimal(),
                Tools.Mapper.Map<TransportModel, TransportDTO>(tourModel.TransportOut),
                tourModel.AvailibleSeatsOut,
                tourModel.PriceForTicketOut.ToDecimal()));

            await _tourService.AddTour(Tools.Mapper
                .Map<TourModel, TourDTO>(tour));

            return RedirectToAction(nameof(ManageTours));
        }

        public async Task<IActionResult> EditTour(int id)
        {
            if (id < 0)
            {
                return NotFound();
            }

            var tour = Tools.Mapper.Map<TourDTO, TourModel>(await _tourService.GetTourAsync(id));
            if (tour == null)
            {
                return NotFound();
            }
            var tourModel = new TourVM()
            {
                Tour = tour,
                TourPrice = tour.Price.ToString(),
                Hotel = tour.Hotel,
                Rooms = tour.Hotel.Rooms,
                TransportIn = tour.TransportIn,
                PriceForTicketIn = tour.TransportIn.TransportPlaces[0].Price.ToString(),
                TransportOut = tour.TransportOut,
                PriceForTicketOut = tour.TransportOut.TransportPlaces[0].Price.ToString(),
            };
            return View(tourModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTour(int id, TourVM tourModel)
        {

            await UploadFile(tourModel.Hotel.Id, tourModel.UploadedFile);
            if (tourModel.TourPrice != null)
            {
                tourModel.Tour.Price = int.Parse(tourModel.TourPrice);
            }

            tourModel.Tour.Hotel = tourModel.Hotel;

            tourModel.Tour.TransportInId = tourModel.TransportIn.Id;

            tourModel.Tour.TransportOutId = tourModel.TransportOut.Id;

            await _transportService.ApplyNewPriceForTicketAndUpdateTransportAsync(Tools.Mapper
                .Map<TransportModel, TransportDTO>(tourModel.TransportIn),
                tourModel.PriceForTicketIn.ToDecimal());

            await _transportService.ApplyNewPriceForTicketAndUpdateTransportAsync(Tools.Mapper
                .Map<TransportModel, TransportDTO>(tourModel.TransportOut),
                tourModel.PriceForTicketOut.ToDecimal());

            if (tourModel.Rooms != null && tourModel.Rooms.Count > 0)
            {
                await _hotelService.UpdateHotelAndHotelRoomsAsync(tourModel.Hotel.Id, Tools.Mapper.
                    Map<HotelModel, HotelDTO>(tourModel.Hotel), Tools.Mapper.
                    Map<List<HotelRoomModel>, List<HotelRoomDTO>>(tourModel.Rooms));
            }

            if (tourModel.NewRoom.Number != 0 || !string.IsNullOrEmpty(tourModel.NewRoom.Name))
            {
                await _hotelService.AddHotelRoomAsync(tourModel.Hotel.Id,
                    Tools.Mapper.Map<HotelRoomModel, HotelRoomDTO>(tourModel.NewRoom));
            }

            await _tourService.UpdateTour(id,
                Tools.Mapper.Map<TourModel, TourDTO>(tourModel.Tour));

            return RedirectToAction(nameof(EditTour));
        }
        public async Task UploadFile(int HotelId, IFormFile uploadedFile)
        {

            if (uploadedFile != null)
            {
                string path = "/Files/" + uploadedFile.FileName;
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
                await _hotelService.InsertImageHotel(HotelId, uploadedFile.FileName, path);
            }
        }

        public async Task<IActionResult> DeleteTour(int id)
        {
            if (id < 0)
            {
                return NotFound();
            }

            var tourModel = await _tourService.GetTourAsync(id);
            if (tourModel == null)
            {
                return NotFound();
            }

            return View(Tools.Mapper.Map<TourDTO, TourModel>(tourModel));
        }

        [HttpPost, ActionName("DeleteTour")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTourConfirmed(int id)
        {
            if (_tourService.GetTourAsync(id) == null)
            {
                return NotFound();
            }

            await _tourService.DeleteTour(id);

            return RedirectToAction(nameof(ManageTours));
        }
    }
}
