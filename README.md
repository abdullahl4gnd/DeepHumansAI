# DeepHumans AI

An innovative ASP.NET Core 8.0 application that enables users to have natural, human-like conversations with AI-powered historical characters.

## Overview

DeepHumans AI brings historical figures to life through advanced AI conversations. Users can interact with 9 different historical personalities, each with unique knowledge, speaking styles, and perspectives. The system maintains conversation history to provide context-aware responses that feel authentic and engaging.

## Features

### 🤖 AI Conversations
- **9 Historical Characters**: Einstein, Kanye West, Winston Churchill, Tupac Shakur, William Shakespeare, Salah al-Din, Queen Hatshepsut, Mustafa Kemal Atatürk, and Saddam Hussein
- **Conversation Memory**: System maintains conversation history for contextual and natural responses
- **Character-Specific Personalities**: Each character has unique speaking style, knowledge, and perspective
- **Advanced AI Parameters**: Optimized temperature, top_p, and prediction length for natural responses

### 🔐 Authentication & Security
- Secure user registration and login with strong password requirements
- Email-based password reset functionality
- Account lockout protection (5 failed attempts)
- Anti-CSRF token protection
- Secure cookie-based authentication

### 📧 Email Integration
- **SendGrid Support**: Primary email service for password resets
- **Gmail SMTP Fallback**: Alternative email delivery method
- Comprehensive error logging and debugging
- HTML-formatted password reset emails with callback links

### 💻 User Interface
- Modern gradient-based design with Bootstrap Icons
- Responsive layout for desktop and mobile
- Form validation with custom JavaScript
- Clean and intuitive user experience
- Account management pages

## Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: MySQL 8.0+ with Entity Framework Core
- **ORM**: Pomelo Entity Framework Core MySql 8.0.2
- **AI**: Ollama API (llama3.2 model)
- **Email**: SendGrid API
- **Frontend**: Bootstrap, jQuery, HTML5, CSS3
- **Authentication**: ASP.NET Core Identity
- **Build Tool**: dotnet CLI

## Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 8.0 SDK** or later
- **MySQL Server 8.0+** running locally
- **Ollama** with llama3.2 model (for AI conversations)
- **Git** for version control
- **Visual Studio Code** or Visual Studio (optional)

## Installation

### 1. Clone the Repository

```bash
git clone https://github.com/abdullahl4gnd/DeepHumansAI.git
cd DeepHumansAI
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Configure Database

Update `appsettings.json` with your MySQL credentials:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Port=3306;Database=deephumansdb;Uid=root;Pwd=your-password;AllowPublicKeyRetrieval=True;AllowUserVariables=True;"
}
```

### 4. Create Database

```bash
dotnet ef database update
```

This will create the `deephumansdb` database and all required tables.

### 5. Configure Email Service (Optional)

For password reset functionality, add your SendGrid API key to `appsettings.json`:

```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": "587",
  "SenderEmail": "your-email@gmail.com",
  "SenderName": "DeepHumans Support",
  "Password": "your-gmail-app-password",
  "SendGridApiKey": "your-sendgrid-api-key",
  "Provider": "sendgrid"
}
```

**Note**: Store sensitive credentials in User Secrets for production:

```bash
dotnet user-secrets init
dotnet user-secrets set "EmailSettings:SendGridApiKey" "your-key-here"
```

### 6. Start Ollama

Ensure Ollama is running with the llama3.2 model:

```bash
ollama serve
# In another terminal:
ollama run llama3.2
```

## Running the Application

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run
```

The application will start at `http://localhost:5287`

### Run in Development

```bash
dotnet watch run
```

## Usage

### User Registration

1. Navigate to the Register page
2. Enter email (must be unique)
3. Create password (minimum 10 characters, requires uppercase, digits, special characters)
4. Confirm password
5. Click Register

### Login

1. Navigate to the Login page
2. Enter your email and password
3. Optionally check "Remember me"
4. Click Login

### Chat with Characters

1. After login, select a character from the available options
2. Type your message
3. Send and receive AI-powered responses
4. Continue conversation - the system remembers previous messages for context

### Password Reset

1. Click "Forgot your password?" on the login page
2. Enter your email address
3. Check your email for the password reset link
4. Click the link and enter your new password
5. Login with your new password

## Project Structure

```
DeepHumansAI/
├── Areas/
│   └── Identity/
│       └── Pages/
│           └── Account/
│               ├── Login.cshtml
│               ├── Register.cshtml
│               ├── ForgotPassword.cshtml
│               └── Manage/
├── Controllers/
│   ├── ChatController.cs
│   └── HomeController.cs
├── Data/
│   └── ApplicationDbContext.cs
├── Models/
│   ├── ApplicationUser.cs
│   ├── ChatMessage.cs
│   └── ErrorViewModel.cs
├── Services/
│   ├── AssistantService.cs
│   └── EmailSender.cs
├── Views/
│   ├── Home/
│   ├── Shared/
│   └── _Layout.cshtml
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── lib/
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
└── DeepHumans.csproj
```

## Character Personalities

### Albert Einstein
Renowned physicist discussing relativity, thought experiments, philosophy, and music.

### Kanye West
Bold rapper and producer discussing music, fashion, creativity, and entrepreneurship.

### Winston Churchill
British statesman discussing leadership, rhetoric, WWII history, and governance.

### Tupac Shakur
Legendary rapper and poet discussing hip-hop, social justice, poetry, and activism.

### William Shakespeare
Greatest playwright discussing theatre, sonnets, love, and human nature.

### Salah al-Din
Islamic leader discussing chivalry, crusades history, honor, and leadership.

### Queen Hatshepsut
Egyptian pharaoh discussing ancient Egypt, trade expeditions, and female leadership.

### Mustafa Kemal Atatürk
Turkish founder discussing modernization, secularism, education, and reform.

### Saddam Hussein
Iraqi president discussed from historical and educational perspective.

## Database Schema

### AspNetUsers
- User authentication and profile information
- Email uniqueness enforced
- Password hash storage

### ChatMessages
- Stores all conversation messages
- Columns: Id, UserId, CharacterName, MessageContent, IsBot, Timestamp
- Enables conversation history and memory

### AspNetRoles, AspNetUserRoles, AspNetUserClaims
- Role-based access control
- User claims management

## API Endpoints

### POST `/api/chat/send`
Send a message and receive AI response

**Request:**
```json
{
  "characterName": "Albert Einstein",
  "messageContent": "What is relativity?"
}
```

**Response:**
```json
{
  "success": true,
  "reply": "AI response text...",
  "characterName": "Albert Einstein"
}
```

## Troubleshooting

### Database Connection Issues

**Error**: "Cannot connect to MySQL server"

**Solution**:
- Ensure MySQL is running: `mysql --version`
- Check connection string in `appsettings.json`
- Verify credentials are correct
- Create database manually if needed

### Ollama Connection Issues

**Error**: "Failed to connect to AI service"

**Solution**:
- Ensure Ollama is running: `ollama serve`
- Check that llama3.2 model is available: `ollama list`
- Verify port 11434 is accessible
- Check firewall settings

### Email Not Sending

**Error**: "Failed to send password reset email"

**Solution**:
- Verify SendGrid API key is correct
- Check that sender email is verified on SendGrid
- Look at application logs for detailed error messages
- Enable `appsettings.Development.json` for detailed logging
- Test with different email address

### Build Failures

**Error**: "CS0136: A local or parameter named 'content' cannot be declared..."

**Solution**:
- Clean build: `dotnet clean && dotnet build`
- Delete bin and obj folders manually
- Update to latest .NET 8 SDK

## Security Considerations

- Passwords are hashed using PBKDF2
- Sensitive data (API keys) should be stored in User Secrets, not in appsettings.json
- CSRF tokens protect all state-changing operations
- Secure cookies are configured with HttpOnly and Secure flags
- Password reset tokens are time-limited and one-time use
- Email addresses must be unique per user

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Future Enhancements

- [ ] Email confirmation for new accounts
- [ ] Two-factor authentication (2FA)
- [ ] Additional historical characters
- [ ] Speech-to-text input
- [ ] Text-to-speech responses
- [ ] Conversation export (PDF/TXT)
- [ ] User-customizable character personalities
- [ ] Multi-language support
- [ ] Advanced analytics and insights

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues, questions, or suggestions:
1. Open an issue on GitHub
2. Check existing issues for similar problems
3. Provide detailed error messages and steps to reproduce

## Acknowledgments

- Ollama for providing the AI model
- SendGrid for email delivery
- ASP.NET Core community
- Bootstrap for UI framework
- All contributors and users

---

**Last Updated**: December 18, 2025

For the latest updates and releases, visit: [GitHub Repository](https://github.com/abdullahl4gnd/DeepHumansAI)
