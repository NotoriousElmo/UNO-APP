using System.ComponentModel.DataAnnotations;
using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages.Games
{
    public class CreateModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IGameRepository _repository;

        public CreateModel(AppDbContext context, IGameRepository repository)
        {
            _context = context;
            _repository = repository;

            // DB
            _repository = new GameRepositoryEF(context);
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty] 
        public Dictionary<ECardValue, int> AmountOfCardValue { get; set; } = new () {
            [ECardValue.Draw4] = 4,
            [ECardValue.ChangeColor] = 4,
            [ECardValue.Plus2] = 2,
            [ECardValue.Reverse] = 2,
            [ECardValue.Skip] = 2,
            [ECardValue.Value0] = 1,
            [ECardValue.Value1] = 2,
            [ECardValue.Value2] = 2,
            [ECardValue.Value3] = 2,
            [ECardValue.Value4] = 2,
            [ECardValue.Value5] = 2,
            [ECardValue.Value6] = 2,
            [ECardValue.Value7] = 2,
            [ECardValue.Value8] = 2,
            [ECardValue.Value9] = 2,
        };

        [BindProperty]
        [Range(2, 10, ErrorMessage = "The number of players must be between 2 and 10.")]
        public int PlayersTotal { get; set; } = 2;
        [BindProperty]
        [Range(0, 10, ErrorMessage = "The number of human players must be between 0 and 10.")]
        public int PlayerTypesHuman { get; set; } = 1;
        [BindProperty]
        [Range(5, 10, ErrorMessage = "Hand size must be between 5 and 10.")]
        public int HandSize { get; set; } = 7;
        [BindProperty]
        public bool SkipAfterDraw { get; set; } = true;
        [BindProperty]
        public bool CanStackCards { get; set; } = true;
        [BindProperty]
        public bool CanPlayDrawWithoutRestrictions { get; set; } = false;

        public async Task<IActionResult> OnPostAsync()
        {

            await _context.SaveChangesAsync();

            if (PlayerTypesHuman > 9)
            {
                PlayerTypesHuman = 10;
                PlayersTotal = 10;
            }
            else if (PlayersTotal > 10)
            {
                PlayersTotal = 10;
            }

            Dictionary<EPlayerType, int> playerTypes = new()
            {
                [EPlayerType.Human] = PlayerTypesHuman,
                [EPlayerType.Bot] = PlayersTotal - PlayerTypesHuman
            };

            foreach (var key in AmountOfCardValue.Keys)
            {
                if (AmountOfCardValue[key] > 10)
                {
                    AmountOfCardValue[key] = 10;
                } else if (AmountOfCardValue[key] < 0)
                {
                    AmountOfCardValue[key] = 0;
                }
            }
            
            var state = new GameState
            {
                PlayerTypes = playerTypes,
                HandSize = HandSize,
                SkipAfterDraw = SkipAfterDraw,
                CanStackCards = CanStackCards,
                AmountOfCardValues = AmountOfCardValue,
                CanPlayDrawWithoutRestrictions = CanPlayDrawWithoutRestrictions
            };

            return RedirectToPage("./Names", new {StateJson = System.Text.Json.JsonSerializer
                .Serialize(state, JsonHelper.JsonSerializerOptions)});
        }
    }
}
