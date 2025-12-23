# Dungeon Party Game

[![Build and Test](https://github.com/mcarthey/DungeonPartyGame/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/mcarthey/DungeonPartyGame/actions/workflows/build-and-test.yml)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A cross-platform .NET MAUI application for managing dungeon party adventures. Create and manage your party of adventurers, assign skills, equip gear, and embark on epic quests!

## 🎮 Features

- **Party Management**: Create and manage your adventuring party
- **Character Selection**: Choose from various character classes and races
- **Skill System**: Assign and manage character skills and abilities
- **Gear Management**: Equip weapons, armor, and magical items
- **Cross-Platform**: Runs on Windows, Android, iOS, and Mac Catalyst
- **Modern UI**: Built with .NET MAUI and XAML for a responsive experience

## 🛠️ Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) with MAUI workload
- For Android development: Android SDK and emulator
- For iOS development: macOS with Xcode

## 🚀 Installation & Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/mcarthey/DungeonPartyGame.git
   cd DungeonPartyGame
   ```

2. **Install MAUI workloads**
   ```bash
   dotnet workload install maui
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the project**
   ```bash
   dotnet build
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

## 📱 Usage

### Party Management
- Navigate to the Party Management section
- Select characters to add to your party
- View and manage party member details

### Character Development
- Assign skills and abilities to characters
- Equip gear and weapons
- Track character progression

### Game Features
- Start new adventures
- Manage inventory
- Track quest progress

## 🏗️ Project Structure

```
DungeonPartyGame/
├── DungeonPartyGame.Core/          # Core business logic and models
│   ├── Models/                     # Data models (Character, Party, etc.)
│   └── Services/                   # Game services (GameEngine, etc.)
├── DungeonPartyGame.UI/            # UI components and ViewModels
│   ├── Pages/                      # XAML pages
│   ├── ViewModels/                 # MVVM ViewModels
│   └── Converters/                 # Value converters
├── DungeonPartyGame.Tests/         # Unit tests
├── Platforms/                      # Platform-specific code
│   ├── Android/
│   ├── iOS/
│   ├── MacCatalyst/
│   └── Windows/
├── Resources/                      # Shared resources (images, fonts, etc.)
└── Properties/                     # Project properties and settings
```

## 🧪 Testing

Run the test suite:

```bash
dotnet test
```

The project includes comprehensive unit tests covering:
- ViewModels and their commands
- Value converters
- Core game logic
- UI interactions

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines

- Follow MVVM architecture patterns
- Write unit tests for new features
- Ensure cross-platform compatibility
- Use meaningful commit messages

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with [.NET MAUI](https://docs.microsoft.com/dotnet/maui)
- Icons and assets from [MauiIcons](https://github.com/AathifMahir/MauiIcons)
- Testing framework: [xUnit](https://xunit.net/)

---

**Happy adventuring!** 🗡️⚔️🛡️</content>
<parameter name="filePath">e:\Documents\dev\repos\DungeonPartyGame\README.md