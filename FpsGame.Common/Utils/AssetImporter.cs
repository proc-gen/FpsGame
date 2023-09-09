using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Framework.Content.Pipeline.Builder;
using System.Reflection;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Assimp.Unmanaged;
using System.Runtime.InteropServices;
using AssimpLibary = Assimp.Unmanaged.AssimpLibrary;

namespace FpsGame.Common.Utils
{
    /// <summary>
    /// An asset loader for MonoGame to import raw files (textures, fbx etc) without needing to build them upfront via the monogame content manager tool.
    /// In other words, use this class to load models and assets directly into MonoGame project.
    /// Author: Ronen Ness
    /// Date: 09/2021
    /// </summary>
    public class AssetImporter
    {
        // graphics device
        GraphicsDevice _graphics;

        // context objects
        PipelineImporterContext _importContext;
        PipelineProcessorContext _processContext;

        // importers
        OpenAssetImporter _openImporter;
        EffectImporter _effectImporter;
        FontDescriptionImporter _fontImporter;
        Dictionary<string, ContentImporter<AudioContent>> _soundImporters = new Dictionary<string, ContentImporter<AudioContent>>();

        // processors
        ModelProcessor _modelProcessor;
        EffectProcessor _effectProcessor;
        FontDescriptionProcessor _fontProcessor;

        // loaded assets caches
        Dictionary<string, Object> _loadedAssets = new Dictionary<string, Object>();

        // default effect to assign to all meshes
        public BasicEffect DefaultEffect;

        /// <summary>
        /// Create the assets loader.
        /// </summary>
        public AssetImporter(GraphicsDevice graphics)
        {
            _graphics = graphics;
            _openImporter = new OpenAssetImporter();
            _effectImporter = new EffectImporter();
            _soundImporters[".wav"] = new WavImporter();
            _soundImporters[".ogg"] = new OggImporter();
            _soundImporters[".wma"] = new WmaImporter();
            _soundImporters[".mp3"] = new Mp3Importer();
            _fontImporter = new FontDescriptionImporter();

            string projectDir = "_proj";
            string outputDir = "_output";
            string intermediateDir = "_inter";
            var pipelineManager = new PipelineManager(projectDir, outputDir, intermediateDir);

            _importContext = new PipelineImporterContext(pipelineManager);
            _processContext = new PipelineProcessorContext(pipelineManager, new PipelineBuildEvent());

            _modelProcessor = new ModelProcessor();
            _effectProcessor = new EffectProcessor();
            _fontProcessor = new FontDescriptionProcessor();

            DefaultEffect = new BasicEffect(_graphics);
        }

        /// <summary>
        /// Clear models cache.
        /// </summary>
        public void ClearCache()
        {
            _loadedAssets.Clear();
        }

        /// <summary>
        /// Validate given path, and attempt to return from cache.
        /// Will return true if found in cache, false otherwise (fromCache will return result).
        /// Will throw exceptions if path not found, or cache contains wrong type.
        /// </summary>
        bool ValidatePathAndGetCached<T>(string assetPath, out T fromCache) where T : class
        {
            // get from cache
            if (_loadedAssets.TryGetValue(assetPath, out object cached))
            {
                fromCache = cached as T;
                if (fromCache == null) { throw new InvalidOperationException($"Asset path found in cache, but has a wrong type! Expected type: '{(typeof(T)).Name}', found type: '{cached.GetType().Name}'."); }
                return true;
            }

            // make sure file exists
            if (!File.Exists(assetPath))
            {
                throw new FileNotFoundException($"{(typeof(T)).Name} asset file '{assetPath}' not found!", assetPath);
            }

            // not found in cache
            fromCache = null;
            return false;
        }

        /// <summary>
        /// Load a model from path.
        /// </summary>
        /// <param name="modelPath">Model file path.</param>
        /// <returns>MonoGame model.</returns>
        public Model LoadModel(string modelPath)
        {
            // validate path and get from cache
            if (ValidatePathAndGetCached(modelPath, out Model cached))
            {
                return cached;
            }

            // load model and convert to model content
            if(!AssimpLibrary.Instance.IsLibraryLoaded)
            {
                var isArm = RuntimeInformation.ProcessArchitecture == Architecture.Arm64;
                var isOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

                if(isArm && isOSX)
                {
                    AssimpLibrary.Instance.LoadLibrary(Directory.GetCurrentDirectory() + "/libassimp.dylib");
                }
            }
            NodeContent node = _openImporter.Import(modelPath, _importContext);

            List<GeometryContent> source = node.Children.Where(a => a is MeshContent)
                .Cast<MeshContent>()
                .ToList()
                .SelectMany(m => m.Geometry)
                .ToList();

            if(node is MeshContent)
            {
                source.AddRange((node as MeshContent).Geometry.ToList());
            }

            Dictionary<string, string> textures = new Dictionary<string, string>();

            foreach (MaterialContent inputMaterial in source.Select((GeometryContent g) => g.Material).Distinct().ToList())
            {
                foreach(var texture in inputMaterial.Textures)
                {
                    texture.Value.Filename = texture.Value.Filename.Remove(0, texture.Value.Filename.IndexOf("Content"));
                    textures.Add(texture.Key, texture.Value.Filename);
                }
            }

            ModelContent modelContent = _modelProcessor.Process(node, _processContext);

            // sanity
            if (modelContent.Meshes.Count == 0)
            {
                throw new FormatException("Model file contains 0 meshes (could it be corrupted or unsupported type?)");
            }

            // extract bones
            var bones = new List<ModelBone>();
            foreach (var boneContent in modelContent.Bones)
            {
                var bone = new ModelBone
                {
                    Transform = boneContent.Transform,
                    Index = bones.Count,
                    Name = boneContent.Name,
                    ModelTransform = modelContent.Root.Transform
                };
                bones.Add(bone);
            }

            // resolve bones hirarchy
            for (var index = 0; index < bones.Count; ++index)
            {
                var bone = bones[index];
                var content = modelContent.Bones[index];
                if (content.Parent != null && content.Parent.Index != -1)
                {
                    bone.Parent = bones[content.Parent.Index];
                    bone.Parent.AddChild(bone);
                }
            }

            // extract meshes
            var meshes = new List<ModelMesh>();
            foreach (var meshContent in modelContent.Meshes)
            {
                // get params
                var name = meshContent.Name;
                var parentBoneIndex = meshContent.ParentBone.Index;
                var boundingSphere = meshContent.BoundingSphere;
                var meshTag = meshContent.Tag;

                // extract parts
                var parts = new List<Tuple<ModelMeshPart, ModelMeshPartContent>>();
                foreach (var partContent in meshContent.MeshParts)
                {
                    // build index buffer
                    IndexBuffer indexBuffer = new IndexBuffer(_graphics, IndexElementSize.ThirtyTwoBits, partContent.IndexBuffer.Count, BufferUsage.WriteOnly);
                    {
                        Int32[] data = new Int32[partContent.IndexBuffer.Count];
                        partContent.IndexBuffer.CopyTo(data, 0);
                        indexBuffer.SetData(data);
                    }

                    // build vertex buffer
                    var vbDeclareContent = partContent.VertexBuffer.VertexDeclaration;
                    List<VertexElement> elements = new List<VertexElement>();
                    foreach (var declareContentElem in vbDeclareContent.VertexElements)
                    {
                        elements.Add(new VertexElement(declareContentElem.Offset, declareContentElem.VertexElementFormat, declareContentElem.VertexElementUsage, declareContentElem.UsageIndex));
                    }
                    var vbDeclare = new VertexDeclaration(elements.ToArray());
                    VertexBuffer vertexBuffer = new VertexBuffer(_graphics, vbDeclare, partContent.NumVertices, BufferUsage.WriteOnly);
                    {
                        vertexBuffer.SetData(partContent.VertexBuffer.VertexData);
                    }

                    // create and add part
#pragma warning disable CS0618 // Type or member is obsolete
                    ModelMeshPart part = new ModelMeshPart()
                    {
                        VertexOffset = partContent.VertexOffset,
                        NumVertices = partContent.NumVertices,
                        PrimitiveCount = partContent.PrimitiveCount,
                        StartIndex = partContent.StartIndex,
                        Tag = partContent.Tag,
                        IndexBuffer = indexBuffer,
                        VertexBuffer = vertexBuffer
                    };
#pragma warning restore CS0618 // Type or member is obsolete
                    parts.Add(new Tuple<ModelMeshPart, ModelMeshPartContent>(part, partContent));
                }

                // create and add mesh to meshes list
                var mesh = new ModelMesh(_graphics, parts.Select(a => a.Item1).ToList())
                {
                    Name = name,
                    BoundingSphere = boundingSphere,
                    Tag = meshTag,
                };
                meshes.Add(mesh);

                foreach (var part in parts)
                {
                    part.Item1.Effect = GenerateEffect(modelPath, modelContent, part.Item1, part.Item2, textures);
                }

                // add to parent bone
                if (parentBoneIndex != -1)
                {
                    mesh.ParentBone = bones[parentBoneIndex];
                    mesh.ParentBone.AddMesh(mesh);
                }
            }

            // create model
            var model = new Model(_graphics, bones, meshes);
            model.Root = bones[modelContent.Root.Index];
            model.Tag = modelContent.Tag;

            // we need to call BuildHierarchy() but its internal, so we use reflection to access it ¯\_(ツ)_/¯
            var methods = model.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            var BuildHierarchy = methods.Where(x => x.Name == "BuildHierarchy" && x.GetParameters().Length == 0).First();
            BuildHierarchy.Invoke(model, null);

            // add to cache and return
            _loadedAssets[modelPath] = model;
            return model;
        }

        /// <summary>
        /// Load an effect from path.
        /// Note: requires the mgfxc dll to work.
        /// </summary>
        /// <param name="effectFile">Effect file path.</param>
        /// <returns>MonoGame Effect.</returns>
        public Effect LoadEffect(string effectFile)
        {
            // validate path and get from cache
            if (ValidatePathAndGetCached(effectFile, out Effect cached))
            {
                return cached;
            }

            // create effect
            var effectContent = _effectImporter.Import(effectFile, _importContext);
            var effectData = _effectProcessor.Process(effectContent, _processContext);
            var dataBuffer = effectData.GetEffectCode();
            var effect = new Effect(_graphics, dataBuffer, 0, dataBuffer.Length);

            // add to cache and return
            _loadedAssets[effectFile] = effect;
            return effect;
        }

        /// <summary>
        /// Load a compiled effect (.fx file that was built via mgfxc) from path.
        /// To build .fx files into compiled shaders:
        /// 1. run `dotnet tool install -g dotnet-mgfxc` to get the building tool.
        /// 2. Run `mgfxc <SourceFile> <OutputFile> /Profile:OpenGL` to build the shader (you can change the Profile param for DX or PS, check out --help).
        /// </summary>
        /// <param name="effectFile">Effect file path.</param>
        /// <returns>MonoGame Effect.</returns>
        public Effect LoadCompiledEffect(string effectFile)
        {
            // validate path and get from cache
            if (ValidatePathAndGetCached(effectFile, out Effect cached))
            {
                return cached;
            }

            // create effect
            byte[] bytecode = File.ReadAllBytes(effectFile);
            var effect = new Effect(_graphics, bytecode, 0, bytecode.Length);

            // add to cache and return
            _loadedAssets[effectFile] = effect;
            return effect;
        }

        /// <summary>
        /// Load a sound effect from file.
        /// </summary>
        /// <param name="soundFile">Sound effect file path.</param>
        /// <returns>MonoGame SoundEffect.</returns>
        public SoundEffect LoadSound(string soundFile)
        {
            // validate path and get from cache
            if (ValidatePathAndGetCached(soundFile, out SoundEffect cached))
            {
                return cached;
            }

            // import audio
            var extension = Path.GetExtension(soundFile).ToLower();
            if (!_soundImporters.ContainsKey(extension))
            {
                throw new InvalidContentException($"Invalid sound file type '{extension}'. Can only load sound files of types: '{string.Join(',', _soundImporters.Keys)}'.");
            }
            var audioContent = _soundImporters[extension].Import(soundFile, _importContext);

            // create sound and return
            byte[] data = new byte[audioContent.Data.Count];
            audioContent.Data.CopyTo(data, 0);
            var sound = new SoundEffect(data, 0, data.Length, audioContent.Format.SampleRate, audioContent.Format.ChannelCount == 1 ? AudioChannels.Mono : AudioChannels.Stereo, audioContent.LoopStart, audioContent.LoopLength);

            // add to cache and return
            _loadedAssets[soundFile] = sound;
            return sound;
        }

        /// <summary>
        /// Load a spritefont from file.
        /// </summary>
        /// <param name="fontFile">Spritefont file path (xml file describing the spritefont).</param>
        /// <returns>MonoGame SpriteFont.</returns>
        public SpriteFont LoadSpriteFont(string fontFile)
        {
            // validate path and get from cache
            if (ValidatePathAndGetCached(fontFile, out SpriteFont cached))
            {
                return cached;
            }

            // import spritefont xml file
            var fontDescription = _fontImporter.Import(fontFile, _importContext);
            var spriteFontContent = _fontProcessor.Process(fontDescription, _processContext);

            // create spritefont
            var textureContent = spriteFontContent.Texture.Mipmaps[0];
            textureContent.TryGetFormat(out SurfaceFormat format);
            Texture2D texture = new Texture2D(_graphics, textureContent.Width, textureContent.Height, false, format);
            texture.SetData(textureContent.GetPixelData());
            List<Rectangle> glyphBounds = spriteFontContent.Glyphs;
            List<Rectangle> cropping = spriteFontContent.Cropping;
            List<char> characters = spriteFontContent.CharacterMap;
            int lineSpacing = spriteFontContent.VerticalLineSpacing;
            float spacing = spriteFontContent.HorizontalSpacing;
            List<Vector3> kerning = spriteFontContent.Kerning;
            char? defaultCharacter = spriteFontContent.DefaultCharacter;
            var sf = new SpriteFont(texture, glyphBounds, cropping, characters, lineSpacing, spacing, kerning, defaultCharacter);

            // add to cache and return
            _loadedAssets[fontFile] = sf;
            return sf;
        }

        /// <summary>
        /// Load a song from file.
        /// </summary>
        /// <param name="songFile">Song file path.</param>
        /// <returns>MonoGame Song.</returns>
        public Song LoadSong(string songFile)
        {
            // validate path and get from cache
            if (ValidatePathAndGetCached(songFile, out Song cached))
            {
                return cached;
            }

            // load song
            var name = Path.GetFileNameWithoutExtension(songFile);
            var song = Song.FromUri(name, new Uri(songFile));

            // add to cache and return
            _loadedAssets[songFile] = song;
            return song;
        }

        /// <summary>
        /// Load a texture from path.
        /// </summary>
        /// <param name="textureFile">Texture file path.</param>
        /// <returns>MonoGame Texture2D.</returns>
        public Texture2D LoadTexture(string textureFile)
        {
            // validate path and get from cache
            if (ValidatePathAndGetCached(textureFile, out Texture2D cached))
            {
                return cached;
            }

            // load texture
            FileStream fileStream = new FileStream(textureFile, FileMode.Open);
            Texture2D loadedTexture = Texture2D.FromStream(_graphics, fileStream);
            fileStream.Dispose();

            // add to cache and return 
            _loadedAssets[textureFile] = loadedTexture;
            return loadedTexture;
        }

        /// <summary>
        /// A method to generate / return effect per mesh part.
        /// </summary>
        /// <param name="modelPath">Model path this part belongs to.</param>
        /// <param name="modelContent">Loaded model raw content.</param>
        /// <param name="part">Part instance we want to create effect for.</param>
        /// <returns>Effect instance or null to use default.</returns>
        public Effect GenerateEffect(string modelPath, ModelContent modelContent, ModelMeshPart part, ModelMeshPartContent partContent, Dictionary<string, string> textures)
        {
            if(partContent?.Material?.Textures?.Count > 0)
            {
                return new BasicEffect(_graphics)
                {
                    Texture = LoadTexture(textures[partContent.Material.Textures.First().Key]),
                    TextureEnabled = true,
                };
            }
            return DefaultEffect;
        }
    }
}
