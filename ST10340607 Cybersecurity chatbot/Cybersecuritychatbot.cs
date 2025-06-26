using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ST10340607_Cybersecurity_chatbot
{
    public class Cybersecuritychatbot : Chatbot
    {
        private string usersName;
        private bool chatbotRun = true;
        private string currentTopic = null;
        private readonly Dictionary<string, string> userMemory = new();
        private readonly Random _random = new();

        public event Action<string> OnMessageReceived;
        public event Action<List<string>> OnBatchMessagesReceived;

        private readonly Dictionary<string, List<string>> keywordResponses = new()// this dictionary is used to store the tips for the topics 
        {
            ["password"] = new() {
                "Use 12+ characters with mixed types and avoid personal info!",
                "Consider a password manager - it's safer than reusing passwords.",
                "Enable two-factor authentication wherever possible.",
                "Regularly update important passwords every 3-6 months.",
                "Use passphrases like 'PurpleTurtle$JumpsHigh' for better security."
            },
            ["phishing"] = new() {
                "Phishing scams often use urgent language to trick you.",
                "Check sender email addresses carefully - look for misspellings.",
                "Never download attachments from suspicious emails.",
                "Legitimate companies will never ask for passwords via email.",
                "Hover over links to see the actual URL before clicking."
            },
            ["browsing"] = new() {
                "Always look for HTTPS and padlock icons in your browser.",
                "Use a VPN on public Wi-Fi to protect your data.",
                "Keep your browser updated to patch security vulnerabilities.",
                "Disable auto-fill for sensitive information in browsers.",
                "Clear cookies regularly to prevent tracking."
            },
            ["malware"] = new() {
                "Keep your operating system and software updated with security patches.",
                "Use a reputable antivirus and anti-malware solution and scan regularly.",
                "Don't download software from untrusted sources.",
                "Be careful with email attachments, even from known senders.",
                "Back up your important data regularly to protect against ransomware."
            },
            ["wifi"] = new() {
                "Always use WPA3 encryption for your home WiFi if available.",
                "Change default router usernames and passwords immediately.",
                "Hide your network SSID to prevent easy discovery.",
                "Use a guest network for visitors and IoT devices.",
                "Regularly update your router's firmware to patch security vulnerabilities."
            }
        };

        private readonly List<string> defaultResponses = new()
        {
            "I'm not sure I understand. Can you try rephrasing?",
            "Could you ask about cybersecurity topics like passwords, phishing, malware, WiFi or safe browsing?",
            "I specialize in cybersecurity - try asking about online safety."
        };

        private readonly Dictionary<string, Action<string>> keywordResponseHandlers;

        public Cybersecuritychatbot()
        {
            keywordResponseHandlers = new()
            {
                ["password"] = RespondToPassword,
                ["phishing"] = RespondToPhishing,
                ["browsing"] = RespondToBrowsing,
                ["malware"] = RespondToMalware,
                ["wifi"] = RespondToWifi
            };
        }

        public void SetUserName(string name)
        {
            usersName = name;
            userMemory["name"] = name;
           // SendMessage($"Hello {name}! How can I help with cybersecurity today?");
        }
        public bool TryHandleTaskCommand(string input, out string taskDescription)
        {
            taskDescription = string.Empty;

            
            string lowerInput = input.ToLower().Trim();

            
            if (lowerInput.Contains("add task") ||
                lowerInput.Contains("create task") ||
                lowerInput.Contains("remind me") ||
                lowerInput.Contains("set reminder") ||
                lowerInput.StartsWith("task "))
            {
                
                if (lowerInput.Contains("add task"))
                    taskDescription = input.Substring(input.IndexOf("add task") + 8).Trim();
                else if (lowerInput.Contains("create task"))
                    taskDescription = input.Substring(input.IndexOf("create task") + 11).Trim();
                else if (lowerInput.Contains("remind me"))
                    taskDescription = input.Substring(input.IndexOf("remind me") + 9).Trim();
                else if (lowerInput.Contains("set reminder"))
                    taskDescription = input.Substring(input.IndexOf("set reminder") + 12).Trim();
                else if (lowerInput.StartsWith("task "))
                    taskDescription = input.Substring(5).Trim();

                
                taskDescription = taskDescription.TrimEnd('.', '!', '?');

                return !string.IsNullOrWhiteSpace(taskDescription);
            }

            return false;
        }

        
        public bool TryHandleQuizCommand(string input)
        {
            string lowerInput = input.ToLower();

            
            return Regex.IsMatch(lowerInput, @"^(start|begin|take|launch)\s+(quiz|test)\b") ||
                   (Regex.IsMatch(lowerInput, @"\b(quiz|test)\b") &&
                   !Regex.IsMatch(lowerInput, @"\b(task|reminder|todo|add|create)\b"));
        }
        public void ProcessInput(string input, bool commandDetected = false) // this method is used to process the input the user gives and triggers the correct methods 
        {
            if (commandDetected) return;

            input = input?.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(input))
                input = input?.Trim().ToLower();
            if (string.IsNullOrWhiteSpace(input))
            {
                SendMessage("Please ask a cybersecurity question.");
                return;
            }
            if (TryHandleQuizCommand(input))
            {
                SendMessage("Ready to test your cybersecurity knowledge? Let's start the quiz!");
                return;
            }
            if (IsExitCommand(input))
            {
                SendMessage($"Goodbye {usersName}, stay secure!");
                chatbotRun = false;
                return;
            }

            if (HandleMemoryCommands(input)) return;

            var emotion = detectEmotion(input);
            ChangeChatbotTone(emotion);

            if (currentTopic != null && HandleFollowUp(input)) return;

            if (IsInterestDeclaration(input, out string topic))
            {
                HandleInterestDeclaration(topic);
                currentTopic = topic;
                return;
            }

            if (HandleSpecificQuestions(input)) return;

            foreach (var handler in keywordResponseHandlers)
            {
                if (input.Contains(handler.Key))
                {
                    currentTopic = handler.Key;
                    handler.Value(handler.Key);
                    return;
                }
            }

            SendMessage(OutputRandomResponses("default"));
        }
        
       
        private void RespondToPassword(string topic) => RespondWithInterest(topic, "password", "Pro tip: Consider using a password manager like Bitwarden or 1Password!");
        private void RespondToPhishing(string topic) => RespondWithInterest(topic, "phishing", "Remember: When in doubt, contact the company directly!");
        private void RespondToBrowsing(string topic) => RespondWithInterest(topic, "browsing", "Did you know: Browser extensions like uBlock Origin improve security!");
        private void RespondToMalware(string topic) => RespondWithInterest(topic, "malware", "Pro tip: Consider using sandboxed environments when testing unknown software!");
        private void RespondToWifi(string topic) => RespondWithInterest(topic, "wifi", "Pro tip: Consider using a VPN even on your home network for extra protection!");

        private void RespondWithInterest(string keyword, string interestCheck, string bonusTip)// this method is used when a user shows intrest in a topic 
        {
            var response = OutputRandomResponses(keyword);
            if (userMemory.TryGetValue("interest", out var interest) && interest == interestCheck)
            {
                response = $"{usersName}, since {interestCheck} interests you: {response}";
                if (_random.NextDouble() > 0.5)
                    response += "\n" + bonusTip;
            }
            SendMessage(response);
        }

        private bool IsExitCommand(string input) => input.Contains("exit") || input.Contains("quit") || input.Contains("end");

        private bool HandleMemoryCommands(string input)// this method is used when a user wants to see what the chabot remebers 
        {
            if (input.Contains("what do you remember") || input.Contains("what you know"))
            {
                var messages = new List<string> { "Here's what I remember:" };
                if (userMemory.ContainsKey("interest"))
                    messages.Add($"- You're interested in {userMemory["interest"]} security");
                if (!string.IsNullOrEmpty(usersName))
                    messages.Add($"- Your name is {usersName}");

                SendBatch(messages);
                return true;
            }

            if (input.Contains("forget what you know") || input.Contains("clear memory"))
            {
                userMemory.Clear();
                SendMessage("I've cleared all stored information.");
                return true;
            }

            return false;
        }

        private bool HandleSpecificQuestions(string input)// this method is used when the user asks a direct question 
        {
            var responses = new Dictionary<string, string>
            {
                ["what is phishing"] = "Phishing is a type of cyberattack where attackers impersonate legitimate institutions to steal sensitive data like login credentials or financial information.",
                ["what is password safety"] = "Password safety means using strong, unique passwords, enabling MFA, and avoiding reuse.",
                ["what is safe passwords"] = "Safe passwords include uppercase, lowercase, numbers, and symbols.",
                ["what is safe browsing"] = "Safe browsing means using HTTPS, avoiding suspicious sites, and keeping your browser updated.",
                ["what is malware"] = "Malware refers to software designed to damage or gain unauthorized access to computers, like viruses and ransomware.",
                ["what is wifi security"] = "WiFi security protects your wireless network using encryption and strong settings.",
                ["spot phishing"] = "Look for generic greetings, urgent language, mismatched URLs, and requests for sensitive info.",
                ["detect malware"] = "Watch for slow performance, pop-ups, strange browser behavior, or missing files.",
                ["secure my wifi"] = "Secure WiFi with WPA3, strong passwords, firmware updates, and firewalls."
            };

            foreach (var pair in responses)
            {
                if (input.Contains(pair.Key))
                {
                    SendMessage(pair.Value);
                    return true;
                }
            }

            if (input.Contains("phishing incident") || input.Contains("victim of phishing"))
            {
                SendBatch(new List<string>
                {
                    "If you've been compromised by phishing:",
                    "1. Change affected passwords immediately",
                    "2. Contact your bank if financial info was shared",
                    "3. Scan devices for malware",
                    "4. Report to the impersonated organization"
                });
                return true;
            }

            if (input.Contains("have malware") || input.Contains("virus"))
            {
                SendBatch(new List<string>
                {
                    "If you suspect malware infection:",
                    "1. Disconnect from the internet",
                    "2. Run a full antivirus scan",
                    "3. Use a second malware removal tool",
                    "4. Back up your important data",
                    "5. Consider resetting your system"
                });
                return true;
            }

            if (input.Contains("wifi was hacked") || input.Contains("wifi compromised"))
            {
                SendBatch(new List<string>
                {
                    "If your WiFi network is compromised:",
                    "1. Change router admin password",
                    "2. Update router firmware",
                    "3. Change your WiFi name and password",
                    "4. Enable WPA3 encryption",
                    "5. Remove unknown connected devices"
                });
                return true;
            }

            return false;
        }

        private bool HandleFollowUp(string input)// this method is used to handle the follow up questions 
        {
            if (input.Contains("more") || input.Contains("explain further") || input.Contains("go on"))
            {
                SendMessage(GetDeeperExplanation(currentTopic));
                return true;
            }

            if (input.Contains("clarify") || input.Contains("confused"))
            {
                SendMessage(GetSimplerExplanation(currentTopic));
                return true;
            }

            if (input.Contains("what about") || input.Contains("how about") || input.Contains("also"))
            {
                SendMessage(GetRelatedTopic(currentTopic));
                return true;
            }

            return false;
        }

        private string GetDeeperExplanation(string topic) => topic switch
        {
            "phishing" => "Advanced phishing attacks may spoof entire websites and use HTTPS. Always double-check URLs.",
            "password" => "Use long passphrases like 'BlueHorse$RunsFast!' and store them in a password manager.",
            "browsing" => "Browser extensions like Privacy Badger help enforce safe browsing.",
            "malware" => "Modern malware may run in memory only. Use behavior monitoring to detect it.",
            "wifi" => "Use MAC filtering and check connected devices regularly.",
            _ => "Let me provide more details on that..."
        };

        private string GetSimplerExplanation(string topic) => topic switch
        {
            "phishing" => "Phishing tricks you into clicking fake links to steal your info.",
            "password" => "Use long, hard-to-guess passwords. Don’t reuse them.",
            "browsing" => "Visit only trusted sites. Use HTTPS.",
            "malware" => "Malware is bad software like viruses.",
            "wifi" => "Keep strangers off your WiFi by using strong settings.",
            _ => "Let me put it in simpler terms..."
        };

        private string GetRelatedTopic(string topic) => topic switch
        {
            "phishing" => "You might also want to learn about 'smishing' (phishing via SMS).",
            "password" => "Two-factor authentication is a great add-on for password safety.",
            "browsing" => "Using a VPN helps hide your activity online.",
            "malware" => "Ransomware is a dangerous form of malware that locks your files.",
            "wifi" => "Watch out for 'evil twin' WiFi hotspots that mimic real networks.",
            _ => "What else are you curious about?"
        };

        private bool IsInterestDeclaration(string input, out string topic)
        {
            input = input.ToLower();
            topic = null;

            if (Regex.IsMatch(input, @"\b(interested in|care about|like|want to learn about)\b"))
            {
                if (input.Contains("phishing")) topic = "phishing";
                else if (input.Contains("password")) topic = "password";
                else if (input.Contains("browsing")) topic = "browsing";
                else if (input.Contains("malware")) topic = "malware";
                else if (input.Contains("wifi")) topic = "wifi";
            }

            return topic != null;
        }

        private void HandleInterestDeclaration(string topic)// this method is used to handle when the user says they interested in somethin g
        {
            RememberUserInterest(topic);

            var responses = new List<string>
            {
                $"Got it! I'll remember you're interested in {topic} security, {usersName}.",
                $"Noted: {topic} is important to you. I’ll focus on that more, {usersName}."
            };

            SendMessage(responses[_random.Next(responses.Count)]);
        }

        public void RememberUserInterest(string topic)
        {
            userMemory["interest"] = topic;
        }

        private string OutputRandomResponses(string topic)
        {
            return keywordResponses.TryGetValue(topic, out var responses)
                ? responses[_random.Next(responses.Count)]
                : defaultResponses[_random.Next(defaultResponses.Count)];
        }

        private void ChangeChatbotTone(string emotion)// changes the tone of the chat bot 
        {
            if (emotion == "worried") SendMessage("I understand this is concerning. Let me help...");
            else if (emotion == "frustrated") SendMessage("Cybersecurity can be frustrating. Let's solve this...");
            else if (emotion == "confused") SendMessage("This can be confusing. Let me explain...");
        }

        private void SendMessage(string message)
        {
            OnMessageReceived?.Invoke(message);
        }

        private void SendBatch(List<string> messages)
        {
            OnBatchMessagesReceived?.Invoke(messages);
        }

        public override string detectEmotion(string input)// used to detect the chsnge in emotion of the user 
        {
            if (input.Contains("worried") || input.Contains("scared")) return "worried";
            if (input.Contains("angry") || input.Contains("frustrated")) return "frustrated";
            if (input.Contains("confused") || input.Contains("unsure")) return "confused";
            return "neutral";
        }
    }
}
