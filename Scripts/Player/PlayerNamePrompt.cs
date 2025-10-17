using UnityEngine;

// Generates and stores random player names using adjective-animal combinations
public class PlayerNamePrompt : MonoBehaviour
{
    [Header("Player Identity")]
    [SerializeField] private string playerPrefsKey = "PlayerName";    // Key used to store name in PlayerPrefs
    
    #region Name Generation Data
    // List of adjectives for the first part of the name
    private readonly string[] adjectives = new string[]
    {
        "Adapted", "Adventurous", "Affectionate", "Agile", "Alert", "Ambitious", "Amiable", "Amused",
        "Amusing", "Ancient", "Angelic", "Anxious", "Apathetic", "Apprehensive", "Ardent", "Artistic",
        "Assertive", "Astonishing", "Astute", "Audacious", "Austere", "Authentic", "Awesome", "Awkward",
        "Azure", "Bashful", "Beaming", "Benevolent", "Benign", "Bewildered", "Bighearted", "Billowing",
        "Bitter", "Blissful", "Blithe", "Boisterous", "Bold", "Bouncy", "Boundless", "Brave",
        "Brawny", "Breezy", "Brief", "Bright", "Brilliant", "Brisk", "Broad", "Broken",
        "Bronze", "Bubbling", "Bulky", "Bumpy", "Buoyant", "Burly", "Bustling", "Busy",
        "Caged", "Calculating", "Callous", "Calm", "Candid", "Capable", "Carefree", "Careful",
        "Careless", "Caring", "Cavernous", "Ceaseless", "Celebrated", "Celestial", "Cerulean", "Champion",
        "Charismatic", "Charming", "Cheeky", "Cheerful", "Cherished", "Chilled", "Chivalrous", "Chubby",
        "Chuckling", "Cinematic", "Circular", "Clairvoyant", "Clammy", "Classic", "Clean", "Clear",
        "Clever", "Climactic", "Clingy", "Close", "Cloudy", "Clueless", "Clumsy", "Coastal",
        "Cobalt", "Coherent", "Cold", "Colorful", "Colossal", "Combative", "Comely", "Comfortable",
        "Comical", "Commanding", "Commendable", "Commercial", "Common", "Compact", "Compassionate", "Competent",
        "Competitive", "Complacent", "Complete", "Complex", "Composed", "Comprehensive", "Compulsive", "Concealed",
        "Conceited", "Conceptual", "Concerned", "Concise", "Conclusive", "Concrete", "Condemned", "Conditional",
        "Confident", "Confidential", "Confused", "Congenial", "Congruent", "Connected", "Conscious", "Consecutive",
        "Conservative", "Considerate", "Consistent", "Conspicuous", "Constant", "Constructive", "Consummate", "Contagious",
        "Contemporary", "Contemptible", "Content", "Contented", "Contiguous", "Continual", "Continuing", "Continuous",
        "Contoured", "Contracted", "Contradictory", "Contrary", "Contributive", "Contrite", "Controlled", "Controversial",
        "Convenient", "Conventional", "Conversational", "Convinced", "Convincing", "Convivial", "Convoluted", "Cool",
        "Cool-headed", "Cooperative", "Coordinated", "Copious", "Coquettish", "Cordial", "Core", "Corner",
        "Corny", "Corporate", "Corpulent", "Correct", "Corrective", "Corrupt", "Corrupted", "Corrupting",
        "Cosmic", "Costly", "Coughing", "Countable", "Courageous", "Courteous", "Covert", "Cowardly",
        "Cozy", "Crabby", "Cracked", "Crafty", "Cranky", "Crash", "Crawling", "Crazy",
        "Creamy", "Creative", "Credible", "Creepy", "Criminal", "Crisp", "Critical", "Crooked",
        "Crowded", "Crucial", "Crude", "Cruel", "Crumbling", "Crunchy", "Crushing", "Crusty",
        "Crying", "Cryptic", "Crystalline", "Cubic", "Cubical", "Cuddly", "Cultivated", "Cultured",
        "Cumbersome", "Cunning", "Curable", "Curative", "Curious", "Curly", "Current", "Cursed",
        "Curt", "Curvaceous", "Curvy", "Customary", "Cut", "Cute", "Cutting", "Cynical",
        "Daffy", "Daft", "Daily", "Dainty", "Damaged", "Damaging", "Damp", "Dancing",
        "Dandy", "Dangerous", "Dapper", "Daring", "Dark", "Darkened", "Dashing", "Dastardly",
        "Dauntless", "Dazed", "Dazzling", "Dead", "Deadly", "Deadpan", "Deafening", "Dear",
        "Dearest", "Debonair", "Decadent", "Decayed", "Deceased", "Deceitful", "Deceiving", "Decent",
        "Decentralized", "Deceptive", "Decimated", "Decipherable", "Decisive", "Declining", "Decorative", "Decorous",
        "Decrepit", "Dedicated", "Deep", "Deeply", "Defeated", "Defective", "Defenseless", "Defensive",
        "Defiant", "Deficient", "Definable", "Definitive", "Deformed", "Deft", "Defunct", "Degenerative",
        "Degrading", "Dehydrated", "Dejected", "Delectable", "Deliberate", "Deliberative", "Delicate", "Delicious",
        "Delighted", "Delightful", "Delinquent", "Delirious", "Deluded", "Demanding", "Demented", "Democratic",
        "Demonic", "Demonstrative", "Demure", "Deniable", "Dense", "Dependable", "Dependent", "Deplorable"
    };

    // List of animal names for the second part of the name
    private readonly string[] animals = new string[]
    {
        "Aardvark", "Albatross", "Alligator", "Alpaca", "Ant", "Anteater", "Antelope", "Ape",
        "Armadillo", "Donkey", "Baboon", "Badger", "Barracuda", "Bat", "Bear", "Beaver",
        "Bee", "Bison", "Boar", "Buffalo", "Butterfly", "Camel", "Capybara", "Caribou",
        "Cassowary", "Cat", "Caterpillar", "Cattle", "Chamois", "Cheetah", "Chicken", "Chimpanzee",
        "Chinchilla", "Chough", "Clam", "Cobra", "Cockroach", "Cod", "Cormorant", "Coyote",
        "Crab", "Crane", "Crocodile", "Crow", "Curlew", "Deer", "Dinosaur", "Pug",
        "Dogfish", "Dolphin", "Dotterel", "Dove", "Dragonfly", "Duck", "Dugong", "Dunlin",
        "Eagle", "Echidna", "Eel", "Eland", "Elephant", "Elk", "Emu", "Falcon",
        "Ferret", "Finch", "Fish", "Flamingo", "Fly", "Fox", "Frog", "Gaur",
        "Gazelle", "Gerbil", "Giraffe", "Gnat", "Gnu", "Goat", "Goldfinch", "Goldfish",
        "Goose", "Gorilla", "Goshawk", "Grasshopper", "Grouse", "Guanaco", "Gull", "Hamster",
        "Hare", "Hawk", "Hedgehog", "Heron", "Herring", "Hippopotamus", "Hornet", "Horse",
        "Human", "Hummingbird", "Hyena", "Ibex", "Ibis", "Jackal", "Jaguar", "Jay",
        "Jellyfish", "Kangaroo", "Kingfisher", "Koala", "Kookaburra", "Kouprey", "Kudu", "Lapwing",
        "Lark", "Lemur", "Leopard", "Lion", "Llama", "Lobster", "Locust", "Loris",
        "Louse", "Lyrebird", "Magpie", "Mallard", "Manatee", "Mandrill", "Mantis", "Marten",
        "Meerkat", "Mink", "Mole", "Mongoose", "Monkey", "Moose", "Mouse", "Mosquito",
        "Mule", "Narwhal", "Newt", "Nightingale", "Octopus", "Okapi", "Opossum", "Oryx",
        "Ostrich", "Otter", "Owl", "Oyster", "Panther", "Parrot", "Partridge", "Peafowl",
        "Pelican", "Penguin", "Pheasant", "Pig", "Pigeon", "Pony", "Porcupine", "Porpoise",
        "Quail", "Quelea", "Quetzal", "Rabbit", "Raccoon", "Rail", "Ram", "Rat",
        "Raven", "Red deer", "Red panda", "Reindeer", "Rhinoceros", "Rook", "Salamander", "Salmon",
        "Sand Dollar", "Sandpiper", "Sardine", "Scorpion", "Seahorse", "Seal", "Shark", "Sheep",
        "Shrew", "Skunk", "Snail", "Snake", "Sparrow", "Spider", "Spoonbill", "Squid",
        "Squirrel", "Starling", "Stingray", "Stinkbug", "Stork", "Swallow", "Swan", "Tapir",
        "Tarsier", "Termite", "Tiger", "Toad", "Trout", "Turkey", "Turtle", "Viper",
        "Vulture", "Wallaby", "Walrus", "Wasp", "Weasel", "Whale", "Wildcat", "Wolf",
        "Wolverine", "Wombat", "Woodcock", "Woodpecker", "Worm", "Wren", "Yak", "Zebra"
    };
    #endregion

    private void Awake()
    {
        // Check if a name already exists
        string savedName = PlayerPrefs.GetString(playerPrefsKey, "");
        if (string.IsNullOrEmpty(savedName))
        {
            // If not, generate and save a new random name
            string newName = GenerateRandomName();
            PlayerPrefs.SetString(playerPrefsKey, newName);
            PlayerPrefs.Save();
        }
    }

    // Generate a random name by combining an adjective and animal
    private string GenerateRandomName()
    {
        string adjective = adjectives[Random.Range(0, adjectives.Length)];
        string animal = animals[Random.Range(0, animals.Length)];
        return $"{adjective}{animal}";
    }
    
    // Get the current player name from PlayerPrefs
    public string GetPlayerName()
    {
        return PlayerPrefs.GetString(playerPrefsKey, "");
    }
}
