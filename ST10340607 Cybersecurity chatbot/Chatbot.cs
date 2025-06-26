using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ST10340607_Cybersecurity_chatbot
{
    public abstract class Chatbot
    {



        // Optional: user interest memory (can be overridden)
        public virtual void userInterest(string topic) { }

        // Optional: emotion detection (can be overridden)
        public virtual string detectEmotion(string input) => "neutral";
    }
}
