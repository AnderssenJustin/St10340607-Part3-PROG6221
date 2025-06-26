using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Media;

namespace ST10340607_Cybersecurity_chatbot
{
    public partial class MainWindow : Window
    {
        private Cybersecuritychatbot chatbot;
        private List<string> activityLog;
        private ObservableCollection<CybersecurityTask> tasks;
        private List<QuizQuestion> quizQuestions;
        private int currentQuestionIndex;
        private int score;
        private bool quizActive;
        private Random random;
        private string chatbotUserName;

        public MainWindow()
        {
            InitializeComponent();
            InitializeApplication();
            UserInput.IsEnabled = false;
            SendButton.IsEnabled = false;
        }

        private void InitializeApplication()// this is the method that gets everything ready and runs the app 
        {
            chatbot = new Cybersecuritychatbot();
            activityLog = new List<string>();
            tasks = new ObservableCollection<CybersecurityTask>();
            TaskListBox.ItemsSource = tasks;
            random = new Random();

            chatbot.OnMessageReceived += message => AddToChatDisplay($"Bot: {message}", "#4CAF50");
            chatbot.OnBatchMessagesReceived += messages =>
            {
                foreach (var msg in messages)
                {
                    AddToChatDisplay($"Bot: {msg}", "#4CAF50");
                }
            };

            InitializeQuizQuestions();

            PlayStartupVoice();

            AddToChatDisplay("Welcome to the Cybersecurity Awareness Chatbot!", "#4CAF50");
            AddToChatDisplay("Please enter your name to get started.", "#FFFFFF");

            LogActivity("Application started");
        }
        private void PlayStartupVoice()// method to play the voice on start up 
        {

            string audioFilePath = @"C:\Users\lab_services_student\Desktop\PROG6221-POE\1744782720344369660p9a3uycs-voicemaker.in-speech.wav";

            SoundPlayer player = new SoundPlayer(audioFilePath);
            player.Play();


        }

        private void InitializeQuizQuestions()// this method is where the questions are stored for the game as well as the correct answer for the question that the user must slect 
        {
            quizQuestions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Question = "What should you do if you receive an email asking for your password?",
                    Options = new[] { "Reply with your password", "Delete the email", "Report the email as phishing", "Ignore it" },
                    CorrectAnswer = 2,
                    Explanation = "Reporting phishing emails helps prevent scams and protects others."
                },
                new QuizQuestion
                {
                    Question = "What makes a strong password?",
                    Options = new[] { "Your birthday", "At least 12 characters with mixed types", "Your pet's name", "123456" },
                    CorrectAnswer = 1,
                    Explanation = "Strong passwords should be at least 12 characters with uppercase, lowercase, numbers, and symbols."
                },
                new QuizQuestion
                {
                    Question = "What is two-factor authentication (2FA)?",
                    Options = new[] { "Using two passwords", "An extra security layer beyond passwords", "Having two email accounts", "Logging in twice" },
                    CorrectAnswer = 1,
                    Explanation = "2FA adds an extra security layer, typically requiring something you know and something you have."
                },
                new QuizQuestion
                {
                    Question = "Which of these is a sign of a phishing email?",
                    Options = new[] { "Perfect grammar", "Urgent threats", "Personalized greeting", "Expected content" },
                    CorrectAnswer = 1,
                    Explanation = "Phishing emails often use urgent language to pressure you into quick action."
                },
                new QuizQuestion
                {
                    Question = "What should you do on public WiFi?",
                    Options = new[] { "Use it for banking", "Use a VPN", "Share passwords", "Download software" },
                    CorrectAnswer = 1,
                    Explanation = "VPNs encrypt your connection, protecting your data on public networks."
                },
                new QuizQuestion
                {
                    Question = "How often should you update your software?",
                    Options = new[] { "Never", "Only when it breaks", "Regularly with security patches", "Once a year" },
                    CorrectAnswer = 2,
                    Explanation = "Regular updates patch security vulnerabilities and keep you protected."
                },
                new QuizQuestion
                {
                    Question = "What is malware?",
                    Options = new[] { "Good software", "Malicious software", "Email software", "Gaming software" },
                    CorrectAnswer = 1,
                    Explanation = "Malware is malicious software designed to damage or gain unauthorized access to systems."
                },
                new QuizQuestion
                {
                    Question = "True or False: It's safe to click links in unexpected emails.",
                    Options = new[] { "True", "False" },
                    CorrectAnswer = 1,
                    Explanation = "Never click links in unexpected emails as they may lead to malicious websites."
                },
                new QuizQuestion
                {
                    Question = "What is the best way to backup important data?",
                    Options = new[] { "Never backup", "Only on your computer", "Use the 3-2-1 rule", "Email to yourself" },
                    CorrectAnswer = 2,
                    Explanation = "The 3-2-1 rule: 3 copies of data, 2 different media types, 1 offsite backup."
                },
                new QuizQuestion
                {
                    Question = "What should you do if you suspect malware on your device?",
                    Options = new[] { "Ignore it", "Disconnect from internet and scan", "Continue using normally", "Share files with others" },
                    CorrectAnswer = 1,
                    Explanation = "Disconnect from internet to prevent data theft and run a full system scan."
                }
            };
        }

        private void SetNameButton_Click(object sender, RoutedEventArgs e)// this is the method that is used when the set name button is clicked 
        {
            string name = NameInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter a valid name.", "Invalid Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            chatbot.SetUserName(name);
            //WelcomeText.Text = $"Welcome {name}! I'm here to help with cybersecurity questions.";


            UserInput.IsEnabled = true;
            SendButton.IsEnabled = true;
            TaskManagerButton.IsEnabled = true;
            QuizButton.IsEnabled = true;
            ActivityLogButton.IsEnabled = true;
            ClearChatButton.IsEnabled = true;
            chatbotUserName = name;
            chatbot.SetUserName(name);


            UserInput.Focus();
            Keyboard.Focus(UserInput);

            AddToChatDisplay($"Welcome {name}! I'm your cybersecurity assistant.", "#4CAF50");
            AddToChatDisplay("You can ask me about passwords, phishing, malware, WiFi security, and safe browsing.", "#FFFFFF");
            AddToChatDisplay("Try commands like 'Add task [description]' or click the feature buttons!", "#B0B0B0");

            LogActivity($"User name set: {name}");

            UserInput.Focus();
        }
        private void SubmitInput(string input)
        {
            string textToProcess = !string.IsNullOrEmpty(UserInput.Text) ? UserInput.Text : input;

            if (string.IsNullOrWhiteSpace(textToProcess))
                return;

            UserInput.Text = textToProcess;
            ProcessUserInput();
            UserInput.Clear();
            UserInput.Focus();
        }



        private void SendButton_Click(object sender, RoutedEventArgs e)// method run when user clicks send in the chat bot 
        {
            SubmitInput(UserInput.Text);
        }

        private void UserInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcessUserInput();
            }
        }

        private async void ProcessUserInput()// this method is used to process the input that is recieved by the user 
        {
            string input = UserInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
                return;

            AddToChatDisplay($"You: {input}", "#2196F3");

            await ProcessWithNLP(input);

            UserInput.Clear();
            UserInput.Focus();
            ChatScrollViewer.ScrollToEnd();
        }


        private async Task ProcessWithNLP(string input)// this method ensure that the chatbot understand diffrent ways a user can ask a question and call the diffrent method 
        {
            await Task.Delay(500);

            string lowerInput = input.ToLower();


            if (ContainsTaskIndicators(lowerInput) && !IsExplicitQuestion(lowerInput))
            {
                HandleImplicitTaskCommand(input);
            }
            else if (IsTaskCommand(lowerInput))
            {
                HandleTaskCommand(input);
            }
            else if (IsActivityLogCommand(lowerInput))
            {
                ShowActivityLogInChat();
            }
            else if (IsQuizCommand(lowerInput))
            {
                if (lowerInput.Contains("start"))
                {
                    QuizButton_Click(null, null);
                }
                else
                {
                    AddToChatDisplay("Bot: Ready to test your cybersecurity knowledge? Click the Quiz button or type 'start quiz'!", "#4CAF50");
                }
            }
            else if (IsMemoryCommand(lowerInput))
            {
                HandleMemoryCommand(lowerInput);
            }
            else
            {
                chatbot.ProcessInput(input);
            }
        }

        private bool ContainsTaskIndicators(string input)
        {
            var taskIndicators = new[] { "enable", "update", "check", "review", "setup", "configure", "install", "backup", "scan", "change" };
            var securityTerms = new[] { "password", "2fa", "two-factor", "antivirus", "firewall", "privacy", "security", "vpn" };

            return taskIndicators.Any(indicator => input.Contains(indicator)) &&
                   securityTerms.Any(term => input.Contains(term));
        }

        private bool IsExplicitQuestion(string input)
        {
            var questionWords = new[] { "what", "how", "why", "when", "where", "who", "can you", "do you", "is", "are" };
            return questionWords.Any(word => input.ToLower().StartsWith(word));
        }

        private void HandleImplicitTaskCommand(string input)
        {
            AddToChatDisplay($"Bot: It sounds like you want to create a task. Should I add: '{input}'?", "#4CAF50");
            AddToChatDisplay("Bot: Type 'yes' to confirm, or rephrase as 'Add task [description]'", "#B0B0B0");


        }
        // the methods below are for the nlp to allow the user to type their input in diffrent ways and the chatbot will understand. 
        private bool IsTaskCommand(string input)
        {
            return Regex.IsMatch(input, @"\b(add task|create task|new task|make task|task|remind me|set reminder|add reminder|create reminder|schedule|todo|to do)\b", RegexOptions.IgnoreCase);
        }

        private bool IsActivityLogCommand(string input)
        {
            return Regex.IsMatch(input, @"\b(show activity|activity log|what have you done|show log|history|my activity|recent activity|log|activities)\b", RegexOptions.IgnoreCase);
        }

        private bool IsQuizCommand(string input)
        {
            return Regex.IsMatch(input, @"\b(quiz|test|game|questions|start quiz|play quiz|cybersecurity quiz|knowledge test)\b", RegexOptions.IgnoreCase);
        }

        private bool IsMemoryCommand(string input)
        {
            return Regex.IsMatch(input, @"\b(what do you remember|what you know|forget|clear memory)\b", RegexOptions.IgnoreCase);
        }

        private void HandleTaskCommand(string input) // this method is used to handle the task the the user creates 
        {
            string taskDescription = ExtractTaskDescription(input);

            if (!string.IsNullOrWhiteSpace(taskDescription))
            {
                int reminderDays = ExtractReminderDays(input);

                var newTask = new CybersecurityTask
                {
                    Title = taskDescription.Length > 50 ? taskDescription.Substring(0, 47) + "..." : taskDescription,
                    Description = taskDescription,
                    CreatedDate = DateTime.Now,
                    ReminderDate = DateTime.Now.AddDays(reminderDays),
                    IsCompleted = false
                };

                tasks.Add(newTask);

                AddToChatDisplay($"Bot: Task added successfully: '{taskDescription}'", "#4CAF50");

                if (reminderDays > 0)
                {
                    string reminderText = reminderDays == 1 ? "tomorrow" : $"in {reminderDays} days";
                    AddToChatDisplay($"Bot: I'll remind you {reminderText} ({DateTime.Now.AddDays(reminderDays):yyyy-MM-dd})", "#2196F3");
                }
                else
                {
                    AddToChatDisplay("Bot: Would you like to set a reminder for this task? Try saying 'remind me in X days'", "#B0B0B0");
                }

                LogActivity($"Task added via chat: {taskDescription} (Reminder: {reminderDays} days)");
            }
            else
            {
                AddToChatDisplay("Bot: I couldn't understand the task description. Try: 'Add task [your task description]'", "#FF6B6B");
                AddToChatDisplay("Bot: Examples: 'Add task enable 2FA' or 'Remind me to update passwords'", "#B0B0B0");
            }
        }

        private string ExtractTaskDescription(string input)// this method is used to extract the info  from when the user adds a task in the chat 
        {
            string lowerInput = input.ToLower();


            var patterns = new[]
            {
        @"add task\s+(.+)",
        @"create task\s+(.+)",
        @"new task\s+(.+)",
        @"remind me to\s+(.+)",
        @"remind me about\s+(.+)",
        @"set reminder for\s+(.+)",
        @"task\s+(.+)",
        @"todo\s+(.+)",
        @"to do\s+(.+)"
    };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(lowerInput, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string description = match.Groups[1].Value.Trim();

                    description = Regex.Replace(description, @"\s+(in \d+ days?|tomorrow|next week|today)$", "", RegexOptions.IgnoreCase).Trim();

                    if (!string.IsNullOrWhiteSpace(description))
                    {

                        return char.ToUpper(description[0]) + description.Substring(1);
                    }
                }
            }


            var simplePatterns = new[] { "task", "reminder", "todo", "to do" };
            foreach (var keyword in simplePatterns)
            {
                int index = lowerInput.IndexOf(keyword);
                if (index >= 0)
                {
                    string remaining = input.Substring(index + keyword.Length).Trim();
                    if (!string.IsNullOrWhiteSpace(remaining))
                    {
                        return char.ToUpper(remaining[0]) + remaining.Substring(1);
                    }
                }
            }

            return "";
        }

        private int ExtractReminderDays(string input)// method to look if the user enters when they would like the reminder 
        {

            Match match = Regex.Match(input, @"in (\d+) days?", RegexOptions.IgnoreCase);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int days))
            {
                return days;
            }

            if (input.ToLower().Contains("tomorrow"))
                return 1;
            if (input.ToLower().Contains("next week"))
                return 7;

            return 7;
        }

        private void HandleMemoryCommand(string input)
        {
            if (input.Contains("forget") || input.Contains("clear memory"))
            {
                AddToChatDisplay("Bot: I've cleared my memory of your preferences.", "#4CAF50");
                LogActivity("Memory cleared by user request");
            }
            else
            {

                AddToChatDisplay("Bot: Here's what I remember about you:", "#4CAF50");
                AddToChatDisplay($"- Your name and our conversation history", "#B0B0B0");
                AddToChatDisplay($"- {tasks.Count} tasks you've created", "#B0B0B0");
                AddToChatDisplay($"- {activityLog.Count} logged activities", "#B0B0B0");
            }
        }

        private void ShowActivityLogInChat()// This method is used to show the activity log of the chatbot in the chat 
        {
            AddToChatDisplay("Bot: Here's your recent activity:", "#4CAF50");

            var recentActivities = activityLog.TakeLast(10).ToList();

            if (recentActivities.Count == 0)
            {
                AddToChatDisplay("- No activities recorded yet", "#B0B0B0");
            }
            else
            {
                for (int i = 0; i < recentActivities.Count; i++)
                {
                    AddToChatDisplay($"{i + 1}. {recentActivities[i]}", "#B0B0B0");
                }
            }

            LogActivity("Activity log viewed via chat");
        }

        /*private string GetChatbotResponse(string input)
        {
            
            string lowerInput = input.ToLower();

            if (lowerInput.Contains("password"))
            {
                LogActivity("Password security information provided");
                return GetRandomResponse(new[]
                {
                    "Use 12+ characters with mixed types and avoid personal info!",
                    "Consider a password manager - it's safer than reusing passwords.",
                    "Enable two-factor authentication wherever possible.",
                    "Regularly update important passwords every 3-6 months."
                });
            }
            else if (lowerInput.Contains("phishing"))
            {
                LogActivity("Phishing awareness information provided");
                return GetRandomResponse(new[]
                {
                    "Phishing scams often use urgent language to trick you.",
                    "Check sender email addresses carefully - look for misspellings.",
                    "Never download attachments from suspicious emails.",
                    "Legitimate companies will never ask for passwords via email."
                });
            }
            else if (lowerInput.Contains("malware"))
            {
                LogActivity("Malware protection information provided");
                return GetRandomResponse(new[]
                {
                    "Keep your operating system and software updated with security patches.",
                    "Use a reputable antivirus and anti-malware solution and scan regularly.",
                    "Don't download software from untrusted sources.",
                    "Back up your important data regularly to protect against ransomware."
                });
            }
            else if (lowerInput.Contains("wifi") || lowerInput.Contains("wi-fi"))
            {
                LogActivity("WiFi security information provided");
                return GetRandomResponse(new[]
                {
                    "Always use WPA3 encryption for your home WiFi if available.",
                    "Change default router usernames and passwords immediately.",
                    "Hide your network SSID to prevent easy discovery.",
                    "Use a guest network for visitors and IoT devices."
                });
            }
            else if (lowerInput.Contains("browsing") || lowerInput.Contains("browser"))
            {
                LogActivity("Safe browsing information provided");
                return GetRandomResponse(new[]
                {
                    "Always look for HTTPS and padlock icons in your browser.",
                    "Use a VPN on public Wi-Fi to protect your data.",
                    "Keep your browser updated to patch security vulnerabilities.",
                    "Clear cookies regularly to prevent tracking."
                });
            }
            else
            {
                return GetRandomResponse(new[]
                {
                    "I'm not sure I understand. Can you try rephrasing?",
                    "Could you ask about cybersecurity topics like passwords, phishing, malware, WiFi or safe browsing?",
                    "I specialize in cybersecurity - try asking about online safety.",
                    "Try using commands like 'Add task [description]' or 'Show activity log'!"
                });
            }
        }*/

        /* private string GetRandomResponse(string[] responses)
         {
             return responses[random.Next(responses.Length)];
         }*/

        private void AddToChatDisplay(string message, string color)
        {
            Dispatcher.Invoke(() =>
            {
                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                var formattedMessage = $"[{timestamp}] {message}";


                var textBlock = new TextBlock
                {
                    Text = formattedMessage,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color)),
                    Margin = new Thickness(5, 2, 5, 2),
                    TextWrapping = TextWrapping.Wrap,
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 12
                };


                if (ChatDisplay.Parent is ScrollViewer scrollViewer &&
                    scrollViewer.Content is StackPanel stackPanel)
                {
                    stackPanel.Children.Add(textBlock);
                }
                else
                {

                    ChatDisplay.Text += formattedMessage + "\n";
                }

                ChatScrollViewer.ScrollToEnd();
            });
        }

        private void LogActivity(string activity)// this method is used to log the activities 
        {
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {activity}";
            activityLog.Add(logEntry);


            if (activityLog.Count > 50)
            {
                activityLog.RemoveAt(0);
            }
        }


        private void TaskManagerButton_Click(object sender, RoutedEventArgs e)// this handles when the task manager button is clicked on the screen 
        {
            TaskManagerOverlay.Visibility = Visibility.Visible;
            LogActivity("Task Manager opened");
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)// this is hanldes when the add task button is clicked 
        {
            string title = TaskTitleInput.Text.Trim();
            string description = TaskDescriptionInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Please enter a task title.", "Missing Title", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                description = title;
            }

            if (!int.TryParse(ReminderDaysInput.Text, out int reminderDays) || reminderDays < 0)
            {
                reminderDays = 7;
            }

            var newTask = new CybersecurityTask
            {
                Title = title,
                Description = description,
                CreatedDate = DateTime.Now,
                ReminderDate = DateTime.Now.AddDays(reminderDays),
                IsCompleted = false
            };

            tasks.Add(newTask);


            TaskTitleInput.Clear();
            TaskDescriptionInput.Clear();
            ReminderDaysInput.Text = "7";

            LogActivity($"Task created via Task Manager: {title}");

            MessageBox.Show($"Task '{title}' added successfully!", "Task Added", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TaskListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)// used to select a task in the task bar 
        {
            bool hasSelection = TaskListBox.SelectedItem != null;
            CompleteTaskButton.IsEnabled = hasSelection;
            DeleteTaskButton.IsEnabled = hasSelection;
        }

        private void CompleteTaskButton_Click(object sender, RoutedEventArgs e)// handles when the complete task button is clicked 
        {
            if (TaskListBox.SelectedItem is CybersecurityTask selectedTask)
            {
                selectedTask.IsCompleted = true;
                selectedTask.CompletedDate = DateTime.Now;

                LogActivity($"Task completed: {selectedTask.Title}");

                MessageBox.Show($"Task '{selectedTask.Title}' marked as completed!", "Task Completed", MessageBoxButton.OK, MessageBoxImage.Information);


                TaskListBox.Items.Refresh();
            }
        }

        private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)// handles the delete task button when it is clicked 
        {
            if (TaskListBox.SelectedItem is CybersecurityTask selectedTask)
            {
                var result = MessageBox.Show($"Are you sure you want to delete the task '{selectedTask.Title}'?",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    tasks.Remove(selectedTask);
                    LogActivity($"Task deleted: {selectedTask.Title}");
                }
            }
        }

        private void CloseTaskManagerButton_Click(object sender, RoutedEventArgs e)
        {
            TaskManagerOverlay.Visibility = Visibility.Collapsed;
        }

        // the below methods are all used to handle when certain button are clicked in the quiz 
        private void QuizButton_Click(object sender, RoutedEventArgs e)
        {
            QuizOverlay.Visibility = Visibility.Visible;
            LogActivity("Quiz opened");
        }

        private void StartQuizButton_Click(object sender, RoutedEventArgs e)
        {
            currentQuestionIndex = 0;
            score = 0;
            quizActive = true;

            StartQuizButton.Visibility = Visibility.Collapsed;
            AnswerPanel.Visibility = Visibility.Visible;
            SubmitAnswerButton.Visibility = Visibility.Visible;

            ShowCurrentQuestion();
            LogActivity("Quiz started");
        }

        private void ShowCurrentQuestion()
        {
            if (currentQuestionIndex < quizQuestions.Count)
            {
                var question = quizQuestions[currentQuestionIndex];

                QuestionCounterText.Text = $"Question {currentQuestionIndex + 1} of {quizQuestions.Count}";
                ScoreText.Text = $"Score: {score}/{currentQuestionIndex}";
                QuestionText.Text = question.Question;

                AnswerA.Content = $"A) {question.Options[0]}";
                AnswerB.Content = $"B) {question.Options[1]}";
                AnswerC.Content = question.Options.Length > 2 ? $"C) {question.Options[2]}" : "";
                AnswerD.Content = question.Options.Length > 3 ? $"D) {question.Options[3]}" : "";

                AnswerA.Visibility = Visibility.Visible;
                AnswerB.Visibility = Visibility.Visible;
                AnswerC.Visibility = question.Options.Length > 2 ? Visibility.Visible : Visibility.Collapsed;
                AnswerD.Visibility = question.Options.Length > 3 ? Visibility.Visible : Visibility.Collapsed;


                AnswerA.IsChecked = false;
                AnswerB.IsChecked = false;
                AnswerC.IsChecked = false;
                AnswerD.IsChecked = false;

                FeedbackText.Visibility = Visibility.Collapsed;
                SubmitAnswerButton.Visibility = Visibility.Visible;
                NextQuestionButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                ShowQuizResults();
            }
        }

        private void SubmitAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedAnswer = GetSelectedAnswer();

            if (selectedAnswer == -1)
            {
                MessageBox.Show("Please select an answer.", "No Answer Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var question = quizQuestions[currentQuestionIndex];
            bool isCorrect = selectedAnswer == question.CorrectAnswer;

            if (isCorrect)
            {
                score++;
                FeedbackText.Text = "Thats correct " + question.Explanation;
                FeedbackText.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)); 
            }
            else
            {
                FeedbackText.Text = "That incorrect " + question.Explanation;
                FeedbackText.Foreground = new SolidColorBrush(Color.FromRgb(255, 107, 107)); 
            }

            FeedbackText.Visibility = Visibility.Visible;
            SubmitAnswerButton.Visibility = Visibility.Collapsed;

            if (currentQuestionIndex < quizQuestions.Count - 1)
            {
                NextQuestionButton.Visibility = Visibility.Visible;
            }
            else
            {
                NextQuestionButton.Content = "Show Results";
                NextQuestionButton.Visibility = Visibility.Visible;
            }

            LogActivity($"Quiz question {currentQuestionIndex + 1} answered: {(isCorrect ? "Correct" : "Incorrect")}");
        }

        private int GetSelectedAnswer()
        {
            if (AnswerA.IsChecked == true) return 0;
            if (AnswerB.IsChecked == true) return 1;
            if (AnswerC.IsChecked == true) return 2;
            if (AnswerD.IsChecked == true) return 3;
            return -1;
        }

        private void NextQuestionButton_Click(object sender, RoutedEventArgs e)
        {
            currentQuestionIndex++;
            if (currentQuestionIndex < quizQuestions.Count)
            {
                ShowCurrentQuestion();
            }
            else
            {
                ShowQuizResults();
            }
        }

        private void ShowQuizResults() // this method is used to show the results of the quix once the user has completed it 
        {
            quizActive = false;

            AnswerPanel.Visibility = Visibility.Collapsed;
            SubmitAnswerButton.Visibility = Visibility.Collapsed;
            NextQuestionButton.Visibility = Visibility.Collapsed;
            FeedbackText.Visibility = Visibility.Visible;
            StartQuizButton.Visibility = Visibility.Visible;
            StartQuizButton.Content = "Restart Quiz";

            double percentage = (double)score / quizQuestions.Count * 100;

            string resultMessage;
            if (percentage >= 80)
            {
                resultMessage = $" Excellent! You scored {score}/{quizQuestions.Count} ({percentage:F0}%)\nYou're a cybersecurity pro";
                FeedbackText.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80));
            }
            else if (percentage >= 60)
            {
                resultMessage = $"Good job! You scored {score}/{quizQuestions.Count} ({percentage:F0}%)\nKeep learning to improve your cybersecurity knowledge";
                FeedbackText.Foreground = new SolidColorBrush(Color.FromRgb(255, 193, 7));
            }
            else
            {
                resultMessage = $"You scored {score}/{quizQuestions.Count} ({percentage:F0}%)\nKeep studying cybersecurity to stay safe online";
                FeedbackText.Foreground = new SolidColorBrush(Color.FromRgb(255, 107, 107));
            }

            QuestionText.Text = "Quiz Complete";
            FeedbackText.Text = resultMessage;

            LogActivity($"Quiz completed: {score}/{quizQuestions.Count} ({percentage:F0}%)");
        }

        private void CloseQuizButton_Click(object sender, RoutedEventArgs e)
        {
            QuizOverlay.Visibility = Visibility.Collapsed;
        }


        private void ActivityLogButton_Click(object sender, RoutedEventArgs e)
        {
            ShowActivityLogDialog();
        }

        private void ShowActivityLogDialog()
        {
            var recentActivities = activityLog.TakeLast(20).ToList();

            string logText = "Recent Activity Log:\n\n";

            if (recentActivities.Count == 0)
            {
                logText += "No activities recorded yet.";
            }
            else
            {
                for (int i = 0; i < recentActivities.Count; i++)
                {
                    logText += $"{i + 1}. {recentActivities[i]}\n";
                }
            }

            MessageBox.Show(logText, "Activity Log", MessageBoxButton.OK, MessageBoxImage.Information);
            LogActivity("Activity log viewed via dialog");
        }

        private void ClearChatButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to clear the chat history?",
                "Confirm Clear", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ChatDisplay.Text = "";
                AddToChatDisplay("Chat cleared. How can I help you with cybersecurity?", "#4CAF50");
                LogActivity("Chat history cleared");
            }
        }


    }


    public class CybersecurityTask
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ReminderDate { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }

        public string ReminderText => IsCompleted
            ? $"Completed on {CompletedDate?.ToString("yyyy-MM-dd")}"
            : $"Reminder: {ReminderDate:yyyy-MM-dd}";
    }

    public class QuizQuestion
    {
        public string Question { get; set; }
        public string[] Options { get; set; }
        public int CorrectAnswer { get; set; }
        public string Explanation { get; set; }
    }
}