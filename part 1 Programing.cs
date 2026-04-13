using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
using System.Speech.Synthesis;
using System.Linq;

namespace CybersecurityChatbot
{
    // ==================== MAIN PROGRAM ====================
    class Program
    {
        static void Main(string[] args)
        {
            // Diagnostic check first
            if (!CheckSpeechAvailable())
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n❌ SPEECH ERROR: No voices installed on this system.");
                Console.WriteLine("Install voices via: Settings > Time & Language > Language > Speech");
                Console.ResetColor();
                Console.WriteLine("\nContinuing without audio...\n");
                Thread.Sleep(2000);
            }

            // Initialize components
            var audioPlayer = new AudioPlayer();
            var asciiArt = new AsciiArt();
            var chatbot = new Chatbot();

            // 1. Play voice greeting
            audioPlayer.PlayerGreeting();

            // 2. Display ASCII art
            asciiArt.DisplayLogo();

            // 3. Start chatbot interaction
            chatbot.Start();

            // Cleanup
            audioPlayer.Dispose();

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        static bool CheckSpeechAvailable()
        {
            try
            {
                using (var synth = new SpeechSynthesizer())
                {
                    return synth.GetInstalledVoices().Count > 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }

    // ==================== AUDIO PLAYER ====================
    public class AudioPlayer : IDisposable
    {
        private SpeechSynthesizer synth;
        private bool hasVoices;

        public AudioPlayer()
        {
            synth = new SpeechSynthesizer();

            // Check available voices
            var voices = synth.GetInstalledVoices();
            hasVoices = voices.Count > 0;

            if (hasVoices)
            {
                synth.SetOutputToDefaultAudioDevice();
                synth.Volume = 100;
                synth.Rate = 0;

                // Select voice with fallback
                try
                {
                    synth.SelectVoiceByHints(VoiceGender.Neutral, VoiceAge.Adult);
                }
                catch
                {
                    try
                    {
                        synth.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
                    }
                    catch
                    {
                        // Fallback to first available voice
                        synth.SelectVoice(voices.First().VoiceInfo.Name);
                    }
                }
            }
        }

        public void PlayerGreeting()
        {
            if (!hasVoices)
            {
                Console.WriteLine("[Audio: No voices installed - continuing silently]");
                return;
            }

            string greeting = "Hello! Welcome to the Cybersecurity Awareness Bot.";
            try
            {
                // Use synchronous Speak to ensure audio completes
                synth.Speak(greeting);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Audio error: {ex.Message}] {greeting}");
            }
        }

        public void Speak(string text)
        {
            if (!hasVoices) return;

            try
            {
                synth.Speak(text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Audio error: {ex.Message}]");
            }
        }

        public void Dispose()
        {
            synth?.Dispose();
        }
    }

    // ==================== ASCII ART ====================
    public class AsciiArt
    {
        public void DisplayLogo()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;

            string logo = @"
    ╔═══════════════════════════════════════════════════════════╗
    ║                                                           ║
    ║     🔒 CYBERSECURITY AWARENESS BOT 🔒                     ║
    ║                                                           ║
    ║        ██████╗██╗   ██╗██████╗ ███████╗██████╗           ║
    ║       ██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗          ║
    ║       ██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝          ║
    ║       ██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗          ║
    ║       ╚██████╗   ██║   ██████╔╝███████╗██║  ██║          ║
    ║        ╚═════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝          ║
    ║                                                           ║
    ║              Protecting South Africa Online               ║
    ║                                                           ║
    ╚═══════════════════════════════════════════════════════════╝
            ";

            Console.WriteLine(logo);
            Console.ResetColor();
            Utilities.PrintDivider();
        }
    }

    // ==================== CHATBOT ====================
    public class Chatbot
    {
        private UserInteraction userInteraction;
        private ResponseSystem responseSystem;
        private AudioPlayer audioPlayer;
        private string userName;

        public Chatbot()
        {
            userInteraction = new UserInteraction();
            responseSystem = new ResponseSystem();
            audioPlayer = new AudioPlayer();
        }

        public void Start()
        {
            userName = userInteraction.GetUserName();
            DisplayWelcomeMessage();
            RunConversationLoop();
        }

        private void DisplayWelcomeMessage()
        {
            string welcome = $"Welcome, {userName}! I'm your Cybersecurity Awareness Assistant.";
            Utilities.PrintSuccess($"\n👋 {welcome}");
            audioPlayer.Speak(welcome);

            Console.WriteLine("I can help you with:");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  • Password safety");
            Console.WriteLine("  • Phishing awareness");
            Console.WriteLine("  • Safe browsing tips");
            Console.WriteLine("  • General cybersecurity questions");
            Console.ResetColor();
            Utilities.PrintDivider();
        }

        private void RunConversationLoop()
        {
            bool running = true;

            while (running)
            {
                Console.Write($"\n[{userName}] > ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    Utilities.PrintWarning("Please enter a valid question or type 'help' for options.");
                    continue;
                }

                string response = responseSystem.GetResponse(input, userName);
                TypeText($"[Bot] > {response}");
                audioPlayer.Speak(response);

                if (input.ToLower().Contains("exit") || input.ToLower().Contains("bye"))
                {
                    string goodbye = $"Stay safe online, {userName}! 🔒";
                    Console.WriteLine($"\n{goodbye}");
                    audioPlayer.Speak(goodbye);
                    running = false;
                }
            }
        }

        private void TypeText(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(15);
            }
            Console.WriteLine();
            Console.ResetColor();
        }
    }

    // ==================== USER INTERACTION ====================
    public class UserInteraction
    {
        public string GetUserName()
        {
            Console.Write("\nPlease enter your name: ");
            string name = Console.ReadLine();

            while (string.IsNullOrWhiteSpace(name))
            {
                Utilities.PrintWarning("Name cannot be empty. Please enter your name: ");
                name = Console.ReadLine();
            }

            return name.Trim();
        }
    }

    // ==================== RESPONSE SYSTEM ====================
    public class ResponseSystem
    {
        private Dictionary<string, string> responses;

        public ResponseSystem()
        {
            InitializeResponses();
        }

        private void InitializeResponses()
        {
            responses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["hello"] = "Hello! I'm here to help you stay safe online. What would you like to know about cybersecurity?",
                ["hi"] = "Hi there! Ready to learn about cybersecurity?",
                ["how are you"] = "I'm just a bot, but I'm fully operational and ready to help you with cybersecurity!",
                ["purpose"] = "I educate South African citizens on cybersecurity threats like phishing, malware, and social engineering.",
                ["what can i ask"] = "You can ask me about: password safety, phishing emails, safe browsing, malware, and social engineering.",
                ["help"] = "Available topics: password, phishing, malware, browsing, social engineering. Or ask me 'how are you' or 'what is your purpose'",
                ["password"] = "🔐 PASSWORD SAFETY:\n• Use at least 12 characters\n• Mix uppercase, lowercase, numbers, and symbols\n• Never reuse passwords across sites\n• Use a password manager\n• Enable two-factor authentication (2FA)",
                ["passwords"] = "Strong passwords are your first defense! Use unique, complex passwords for each account.",
                ["phishing"] = "🎣 PHISHING AWARENESS:\n• Check sender email addresses carefully\n• Hover over links before clicking\n• Look for urgent language ('Act now!')\n• Never download unexpected attachments\n• Verify requests for personal info",
                ["email"] = "Suspicious emails often have: spelling errors, generic greetings ('Dear Customer'), and urgent threats.",
                ["browsing"] = "🌐 SAFE BROWSING:\n• Look for 'https://' and padlock icon\n• Avoid public WiFi for banking\n• Keep browsers updated\n• Don't click pop-ups saying you 'won a prize'",
                ["internet"] = "Always verify website URLs. Scammers create fake sites that look like real banks or stores.",
                ["malware"] = "🦠 MALWARE PROTECTION:\n• Install reputable antivirus software\n• Keep all software updated\n• Don't download from untrusted sources\n• Backup your data regularly",
                ["virus"] = "Protect against viruses by: updating software, avoiding suspicious downloads, and using antivirus.",
                ["social engineering"] = "🎭 SOCIAL ENGINEERING:\n• Be skeptical of unexpected calls/emails\n• Verify identities before sharing info\n• Don't let urgency pressure you\n• 'IT support' won't ask for your password",
                ["default"] = "I didn't quite understand that. Could you rephrase? Try asking about: password, phishing, malware, or browsing."
            };
        }

        public string GetResponse(string input, string userName)
        {
            input = input.ToLower().Trim();

            if (responses.ContainsKey(input))
            {
                return responses[input];
            }

            foreach (var keyword in responses.Keys)
            {
                if (input.Contains(keyword) && keyword != "default")
                {
                    return responses[keyword];
                }
            }

            return responses["default"];
        }
    }

    // ==================== UTILITIES ====================
    public static class Utilities
    {
        public static void PrintDivider()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("═══════════════════════════════════════════════════════════");
            Console.ResetColor();
        }

        public static void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void PrintWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠️  {message}");
            Console.ResetColor();
        }

        public static void PrintError(string message)S
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ {message}");
            Console.ResetColor();
        }
    }
}