### .github/copilot-instructions.md

# RimWorld Mod: Soft Warm Beds

## Mod Overview and Purpose

The "Soft Warm Beds" mod for RimWorld enhances the existing in-game system around beds to create a more immersive and dynamic sleeping experience for colonists. It introduces new mechanics involving bedding and bed customization, adding depth to comfort and rest systems in the game. This mod aims to diversify the player's strategic options related to colonist comfort and improve immersion through additional gameplay mechanics.

## Key Features and Systems

- **Makeable Beds:** Transition existing beds into makeable ones, allowing for customization and integration of different beddings.
- **Bedding System:** Introduces a variety of bedding types, each providing unique attributes such as softness and warmth.
- **Storage Settings for Bedding:** Customizable storage settings to manage bedding resources effectively.
- **Enhanced Rest Dynamics:** Alters comfort levels and rest quality based on the type of bedding used by colonists.
- **Harmony Patches:** Several Harmony patches are integrated to tweak existing game behavior relating to beds, rest, and comfort systems.
- **Job System Enhancement:** New work types and job definitions related to bed making and maintenance.
- **Integration with Existing Mods:** Compatibility patches for popular mods like Hospitality, ensuring consistency across gameplay mechanics.

## Coding Patterns and Conventions

- **Class Naming:** Classes are named with descriptive titles that clearly indicate their purpose within the mod. For instance, `CompMakeableBed` indicates its purpose as a component for makeable beds.
- **Method Naming:** Methods use PascalCase and are named with respect to their function, like `DrawBed` and `LoadBedding`.
- **Interfaces:** Implements interfaces such as `IStoreSettingsParent` for classes that need to manage storage settings.
- **Inheritance:** Uses inheritance where applicable. `CompMakeableBed` extends `CompFlickable`, borrowing functionality while adding new features.

## XML Integration

- XML definitions are used to outline new items, jobs, and recipes introduced by the mod.
- XML files work in tandem with C# classes to define the properties of new items and manage gameplay behavior.
- Leverages PatchOperation classes such as `PatchOperationCopy` to modify existing XML definitions seamlessly.

## Harmony Patching

- Harmony patches are extensively used to modify core game behavior without altering the original game files.
- A selection of static and non-static patch classes, such as `Hospitality_Patch` and `TryGainMemory_Patch`, demonstrate methods to implement runtime changes.
- Commonly patched methods involve rendering of beds, memory gains, and guest management, ensuring the mod's features integrate smoothly into the base game.

## Suggestions for Copilot

1. **Expanding Bed Types:** Suggest Copilot to generate additional bedding types with varying attributes for more gameplay diversity.
2. **Integration Suggestions:** Use Copilot to propose methods for integrating additional mods seamlessly.
3. **Enhanced Debugging:** Suggest generating test cases or debugging logs that could assist in catching integration errors or patch conflicts.
4. **Performance Optimization:** Leverage Copilot to recommend optimizations for frequently executed methods such as those in the `JobDriver_MakeBed` class.
5. **XML Enhancements:** Generate XPath queries or XML validation routines to ensure all XML definitions comply with expected standards.
6. **User Interface Improvements:** Utilize Copilot to enhance user interface components, offering suggestions for improving tab displays or storage management views associated with beds and bedding.
  
By following these instructions, developers can efficiently leverage GitHub Copilot to enhance the Soft Warm Beds mod, ensuring a polished and immersive experience for RimWorld players.
