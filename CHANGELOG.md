<a name="changelog-top"></a>

# Changelog

Here you can find the release notes of each version of the project from more recent to older ones. 

# v1.0.0 (29/03/2023)

- This was the first release
- Added a single attribute to allow classes generation: `DtoPropertyAttribute`
- Add `using Arisoul.SourceGenerators.DataTransferObjects;` to the top of your class
- Decorate the properties you want to be considered in generation with the attribute `[DtoProperty]`
- Or to costumize the name of the property generated use `[DtoProperty("CustomName")]` or `[DtoProperty(Name = "CustomName")]`