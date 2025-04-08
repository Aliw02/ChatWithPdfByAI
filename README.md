# PDF-TelegramBot

## Overview
PDF-TelegramBot is a .NET 9 application that allows users to interact with a Telegram bot to manage and manipulate PDF files. The bot can perform various operations such as merging, splitting, and converting PDFs.

## Features
- Merge multiple PDF files into one.
- Split a PDF file into multiple files.
- Convert images to PDF.
- Extract text from PDF files.

## Prerequisites
- .NET 9 SDK
- Telegram Bot API token

## Getting Started

### Clone the repository
### Configure the Bot
1. Create a new bot on Telegram and get the API token.
2. Set the API token in the `appsettings.json` file.

### Build and Run
## Usage
1. Start a chat with your bot on Telegram.
2. Use the following commands to interact with the bot:
   - `/merge` - Merge multiple PDF files.
   - `/split` - Split a PDF file.
   - `/convert` - Convert images to PDF.

## Contributing
Contributions are welcome! Please open an issue or submit a pull request.

## License
This project is licensed under the MIT License.
# PDF-TelegramBot

## Overview
PDF-TelegramBot is a .NET 9 application that allows users to interact with a Telegram bot to manage and manipulate PDF files. The bot can perform various operations such as merging, splitting, and converting PDFs.

## Features
- **Merge PDFs**: Combine multiple PDF files into a single document.
- **Split PDFs**: Divide a PDF file into multiple smaller files.
- **Convert Images to PDF**: Transform image files into PDF format.

## Prerequisites
- .NET 9 SDK
- Telegram Bot API token
- Gemini API key for chat with pdf documents
- IronPdf license key for pdf manipulation

## Getting Started

### Clone the Repository
### Configure the Bot
1. Create a new bot on Telegram and obtain the API token.
2. Set the API token in the `.env` file:
3. ```bash 
   APISETTINGS__APITOKEN=your_bot_token_here
   ```

### Build and Run
1. Build the project:
2. Run the project:


## Features
- **Merge PDFs**: Combine multiple PDF files into a single document.
- **Split PDFs**: Divide a PDF file into multiple smaller files.
- **Convert Images to PDF**: Transform image files into PDF format.
- **Encrypt/Decrypt PDFs**: Secure PDF files with passwords or remove existing passwords.

## Usage
1. Start a chat with your bot on Telegram.
2. Use the following commands or buttons to interact with the bot:
   - **Commands**:
     - `/merge` - Merge multiple PDF files.
     - `/split` - Split a PDF file.
     - `/convert` - Convert to PDF Or From PDF.
     - `/encrypt` - Encrypt a PDF file.
     - `/decrypt` - Decrypt a PDF file.
   - **Buttons**:
     - The bot will present buttons for common actions such as "Merge PDFs", "Split PDF", "Convert to PDF", etc. Simply click the button corresponding to the action you want to perform.

## Contributing
Contributions are welcome! Please open an issue or submit a pull request.

## License
This project is licensed under the MIT License.
