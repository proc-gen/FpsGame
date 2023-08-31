using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Assimp;
using Assimp.Configs;
using Quaternion = Microsoft.Xna.Framework.Quaternion;

namespace FpsGame.Common.Utils
{
    [ContentImporter(new string[]
    {
        ".dae", ".gltf", "glb", ".blend", ".3ds", ".ase", ".obj", ".ifc", ".xgl", ".zgl",
        ".ply", ".dxf", ".lwo", ".lws", ".lxo", ".stl", ".ac", ".ms3d", ".cob", ".scn",
        ".bvh", ".csm", ".irrmesh", ".irr", ".mdl", ".md2", ".md3", ".pk3", ".mdc", ".md5",
        ".smd", ".vta", ".ogex", ".3d", ".b3d", ".q3d", ".q3s", ".nff", ".off", ".ter",
        ".hmp", ".ndo"
    }, DisplayName = "Open Asset Import Library Net - MonoGame", DefaultProcessor = "ModelProcessor")]
    public class OpenAssetImporterNet : ContentImporter<NodeContent>
    {
        private class FbxPivot
        {
            public static readonly FbxPivot Default = new FbxPivot();

            public Matrix? Translation;

            public Matrix? RotationOffset;

            public Matrix? RotationPivot;

            public Matrix? PreRotation;

            public Matrix? Rotation;

            public Matrix? PostRotation;

            public Matrix? RotationPivotInverse;

            public Matrix? ScalingOffset;

            public Matrix? ScalingPivot;

            public Matrix? Scaling;

            public Matrix? ScalingPivotInverse;

            public Matrix? GeometricTranslation;

            public Matrix? GeometricRotation;

            public Matrix? GeometricScaling;

            public Matrix GetTransform(Vector3? scale, Quaternion? rotation, Vector3? translation)
            {
                Matrix identity = Matrix.Identity;
                if (GeometricScaling.HasValue)
                {
                    identity *= GeometricScaling.Value;
                }

                if (GeometricRotation.HasValue)
                {
                    identity *= GeometricRotation.Value;
                }

                if (GeometricTranslation.HasValue)
                {
                    identity *= GeometricTranslation.Value;
                }

                if (ScalingPivotInverse.HasValue)
                {
                    identity *= ScalingPivotInverse.Value;
                }

                if (scale.HasValue)
                {
                    identity *= Matrix.CreateScale(scale.Value);
                }
                else if (Scaling.HasValue)
                {
                    identity *= Scaling.Value;
                }

                if (ScalingPivot.HasValue)
                {
                    identity *= ScalingPivot.Value;
                }

                if (ScalingOffset.HasValue)
                {
                    identity *= ScalingOffset.Value;
                }

                if (RotationPivotInverse.HasValue)
                {
                    identity *= RotationPivotInverse.Value;
                }

                if (PostRotation.HasValue)
                {
                    identity *= PostRotation.Value;
                }

                if (rotation.HasValue)
                {
                    identity *= Matrix.CreateFromQuaternion(rotation.Value);
                }
                else if (Rotation.HasValue)
                {
                    identity *= Rotation.Value;
                }

                if (PreRotation.HasValue)
                {
                    identity *= PreRotation.Value;
                }

                if (RotationPivot.HasValue)
                {
                    identity *= RotationPivot.Value;
                }

                if (RotationOffset.HasValue)
                {
                    identity *= RotationOffset.Value;
                }

                if (translation.HasValue)
                {
                    identity *= Matrix.CreateTranslation(translation.Value);
                }
                else if (Translation.HasValue)
                {
                    identity *= Translation.Value;
                }

                return identity;
            }
        }

        private static readonly List<VectorKey> EmptyVectorKeys = new List<VectorKey>();

        private static readonly List<QuaternionKey> EmptyQuaternionKeys = new List<QuaternionKey>();

        private ContentImporterContext _context;

        private ContentIdentity _identity;

        private Scene _scene;

        private Dictionary<string, Matrix> _deformationBones;

        private Node _rootBone;

        private List<Node> _bones = new List<Node>();

        private Dictionary<string, FbxPivot> _pivots;

        private NodeContent _rootNode;

        private List<MaterialContent> _materials;

        private readonly bool _xnaCompatible;

        private readonly string _importerName;

        public bool XnaComptatible { get; set; }

        public OpenAssetImporterNet()
            : this("OpenAssetImporterNet", xnaCompatible: false)
        {
        }

        internal OpenAssetImporterNet(string importerName, bool xnaCompatible)
        {
            _importerName = importerName;
            _xnaCompatible = xnaCompatible;
        }

        public override NodeContent Import(string filename, ContentImporterContext context)
        {
            if (filename == null)
            {
                throw new ArgumentNullException("filename");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            _context = context;

            _identity = new ContentIdentity(filename, _importerName);
            using (AssimpContext assimpContext = new AssimpContext())
            {
                assimpContext.SetConfig(new RemoveDegeneratePrimitivesConfig(removeDegenerates: true));
                _scene = assimpContext.ImportFile(filename, PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.Triangulate | PostProcessSteps.ImproveCacheLocality | PostProcessSteps.FindDegenerates | PostProcessSteps.FindInvalidData | PostProcessSteps.OptimizeMeshes | PostProcessSteps.FlipUVs | PostProcessSteps.FlipWindingOrder);
                FindSkeleton();
                if (_xnaCompatible)
                {
                    ImportXnaMaterials();
                }
                else
                {
                    ImportMaterials();
                }

                ImportNodes();
                ImportSkeleton();
                if (_rootNode.Children.Count == 1 && _rootNode.Children[0] is MeshContent)
                {
                    Matrix absoluteTransform = _rootNode.Children[0].AbsoluteTransform;
                    _rootNode = _rootNode.Children[0];
                    _rootNode.Identity = _identity;
                    _rootNode.Transform = absoluteTransform;
                }

                _scene.Clear();
            }

            return _rootNode;
        }

        private void ImportXnaMaterials()
        {
            _materials = new List<MaterialContent>();
            foreach (Material material in _scene.Materials)
            {
                BasicMaterialContent basicMaterialContent = new BasicMaterialContent
                {
                    Name = material.Name,
                    Identity = _identity
                };
                if (material.HasTextureDiffuse)
                {
                    basicMaterialContent.Texture = ImportTextureContentRef(material.TextureDiffuse);
                }

                if (material.HasTextureOpacity)
                {
                    basicMaterialContent.Textures.Add("Transparency", ImportTextureContentRef(material.TextureOpacity));
                }

                if (material.HasTextureSpecular)
                {
                    basicMaterialContent.Textures.Add("Specular", ImportTextureContentRef(material.TextureSpecular));
                }

                if (material.HasTextureHeight)
                {
                    basicMaterialContent.Textures.Add("Bump", ImportTextureContentRef(material.TextureHeight));
                }

                if (material.HasColorDiffuse)
                {
                    basicMaterialContent.DiffuseColor = ToXna(material.ColorDiffuse);
                }

                if (material.HasColorEmissive)
                {
                    basicMaterialContent.EmissiveColor = ToXna(material.ColorEmissive);
                }

                if (material.HasOpacity)
                {
                    basicMaterialContent.Alpha = material.Opacity;
                }

                if (material.HasColorSpecular)
                {
                    basicMaterialContent.SpecularColor = ToXna(material.ColorSpecular);
                }

                if (material.HasShininessStrength)
                {
                    basicMaterialContent.SpecularPower = material.Shininess;
                }

                _materials.Add(basicMaterialContent);
            }
        }

        private ExternalReference<TextureContent> ImportTextureContentRef(TextureSlot textureSlot)
        {
            ExternalReference<TextureContent> externalReference = new ExternalReference<TextureContent>(textureSlot.FilePath, _identity);
            externalReference.OpaqueData.Add("TextureCoordinate", $"TextureCoordinate{textureSlot.UVIndex}");
            if (!_xnaCompatible)
            {
                externalReference.OpaqueData.Add("Operation", textureSlot.Operation.ToString());
                externalReference.OpaqueData.Add("AddressU", textureSlot.WrapModeU.ToString());
                externalReference.OpaqueData.Add("AddressV", textureSlot.WrapModeU.ToString());
                externalReference.OpaqueData.Add("Mapping", textureSlot.Mapping.ToString());
            }

            return externalReference;
        }

        private void ImportMaterials()
        {
            _materials = new List<MaterialContent>();
            foreach (Material material in _scene.Materials)
            {
                MaterialContent materialContent = new MaterialContent
                {
                    Name = material.Name,
                    Identity = _identity
                };
                TextureSlot[] allMaterialTextures = material.GetAllMaterialTextures();
                for (int i = 0; i < allMaterialTextures.Length; i++)
                {
                    TextureSlot textureSlot = allMaterialTextures[i];
                    string text = ((textureSlot.TextureType != TextureType.Diffuse) ? textureSlot.TextureType.ToString() : "Texture");
                    if (textureSlot.TextureIndex > 0)
                    {
                        text += textureSlot.TextureIndex + 1;
                    }

                    materialContent.Textures.Add(text, ImportTextureContentRef(textureSlot));
                }

                if (material.HasBlendMode)
                {
                    materialContent.OpaqueData.Add("BlendMode", material.BlendMode.ToString());
                }

                if (material.HasBumpScaling)
                {
                    materialContent.OpaqueData.Add("BumpScaling", material.BumpScaling);
                }

                if (material.HasColorAmbient)
                {
                    materialContent.OpaqueData.Add("AmbientColor", ToXna(material.ColorAmbient));
                }

                if (material.HasColorDiffuse)
                {
                    materialContent.OpaqueData.Add("DiffuseColor", ToXna(material.ColorDiffuse));
                }

                if (material.HasColorEmissive)
                {
                    materialContent.OpaqueData.Add("EmissiveColor", ToXna(material.ColorEmissive));
                }

                if (material.HasColorReflective)
                {
                    materialContent.OpaqueData.Add("ReflectiveColor", ToXna(material.ColorReflective));
                }

                if (material.HasColorSpecular)
                {
                    materialContent.OpaqueData.Add("SpecularColor", ToXna(material.ColorSpecular));
                }

                if (material.HasColorTransparent)
                {
                    materialContent.OpaqueData.Add("TransparentColor", ToXna(material.ColorTransparent));
                }

                if (material.HasOpacity)
                {
                    materialContent.OpaqueData.Add("Opacity", material.Opacity);
                }

                if (material.HasReflectivity)
                {
                    materialContent.OpaqueData.Add("Reflectivity", material.Reflectivity);
                }

                if (material.HasShadingMode)
                {
                    materialContent.OpaqueData.Add("ShadingMode", material.ShadingMode.ToString());
                }

                if (material.HasShininess)
                {
                    materialContent.OpaqueData.Add("Shininess", material.Shininess);
                }

                if (material.HasShininessStrength)
                {
                    materialContent.OpaqueData.Add("ShininessStrength", material.ShininessStrength);
                }

                if (material.HasTwoSided)
                {
                    materialContent.OpaqueData.Add("TwoSided", material.IsTwoSided);
                }

                if (material.HasWireFrame)
                {
                    materialContent.OpaqueData.Add("WireFrame", material.IsWireFrameEnabled);
                }

                _materials.Add(materialContent);
            }
        }

        private void ImportNodes()
        {
            _pivots = new Dictionary<string, FbxPivot>();
            _rootNode = ImportNodes(_scene.RootNode, null, null);
        }

        private NodeContent ImportNodes(Node aiNode, Node aiParent, NodeContent parent)
        {
            NodeContent nodeContent = null;
            if (aiNode.HasMeshes)
            {
                MeshContent meshContent = new MeshContent
                {
                    Name = aiNode.Name,
                    Identity = _identity,
                    Transform = ToXna(GetRelativeTransform(aiNode, aiParent))
                };
                foreach (int meshIndex in aiNode.MeshIndices)
                {
                    Mesh mesh = _scene.Meshes[meshIndex];
                    if (mesh.HasVertices)
                    {
                        GeometryContent item = CreateGeometry(meshContent, mesh);
                        meshContent.Geometry.Add(item);
                    }
                }

                nodeContent = meshContent;
            }
            else if (aiNode.Name.Contains("_$AssimpFbx$"))
            {
                string nodeName = GetNodeName(aiNode.Name);
                if (!_pivots.TryGetValue(nodeName, out var value))
                {
                    value = new FbxPivot();
                    _pivots.Add(nodeName, value);
                }

                Matrix value2 = ToXna(aiNode.Transform);
                if (aiNode.Name.EndsWith("_Translation"))
                {
                    value.Translation = value2;
                }
                else if (aiNode.Name.EndsWith("_RotationOffset"))
                {
                    value.RotationOffset = value2;
                }
                else if (aiNode.Name.EndsWith("_RotationPivot"))
                {
                    value.RotationPivot = value2;
                }
                else if (aiNode.Name.EndsWith("_PreRotation"))
                {
                    value.PreRotation = value2;
                }
                else if (aiNode.Name.EndsWith("_Rotation"))
                {
                    value.Rotation = value2;
                }
                else if (aiNode.Name.EndsWith("_PostRotation"))
                {
                    value.PostRotation = value2;
                }
                else if (aiNode.Name.EndsWith("_RotationPivotInverse"))
                {
                    value.RotationPivotInverse = value2;
                }
                else if (aiNode.Name.EndsWith("_ScalingOffset"))
                {
                    value.ScalingOffset = value2;
                }
                else if (aiNode.Name.EndsWith("_ScalingPivot"))
                {
                    value.ScalingPivot = value2;
                }
                else if (aiNode.Name.EndsWith("_Scaling"))
                {
                    value.Scaling = value2;
                }
                else if (aiNode.Name.EndsWith("_ScalingPivotInverse"))
                {
                    value.ScalingPivotInverse = value2;
                }
                else if (aiNode.Name.EndsWith("_GeometricTranslation"))
                {
                    value.GeometricTranslation = value2;
                }
                else if (aiNode.Name.EndsWith("_GeometricRotation"))
                {
                    value.GeometricRotation = value2;
                }
                else
                {
                    if (!aiNode.Name.EndsWith("_GeometricScaling"))
                    {
                        throw new InvalidContentException($"Unknown $AssimpFbx$ node: \"{aiNode.Name}\"", _identity);
                    }

                    value.GeometricScaling = value2;
                }
            }
            else if (!_bones.Contains(aiNode))
            {
                nodeContent = new NodeContent
                {
                    Name = aiNode.Name,
                    Identity = _identity,
                    Transform = ToXna(GetRelativeTransform(aiNode, aiParent))
                };
            }

            if (nodeContent != null)
            {
                parent?.Children.Add(nodeContent);
                aiParent = aiNode;
                parent = nodeContent;
                if (_scene.HasAnimations)
                {
                    foreach (Animation animation in _scene.Animations)
                    {
                        AnimationContent animationContent = ImportAnimation(animation, nodeContent.Name);
                        if (animationContent.Channels.Count > 0)
                        {
                            nodeContent.Animations.Add(animationContent.Name, animationContent);
                        }
                    }
                }
            }

            foreach (Node child in aiNode.Children)
            {
                ImportNodes(child, aiParent, parent);
            }

            return nodeContent;
        }

        private GeometryContent CreateGeometry(MeshContent mesh, Mesh aiMesh)
        {
            GeometryContent geometryContent = new GeometryContent
            {
                Identity = _identity,
                Material = _materials[aiMesh.MaterialIndex]
            };
            int count = mesh.Positions.Count;
            foreach (Vector3D vertex in aiMesh.Vertices)
            {
                mesh.Positions.Add(ToXna(vertex));
            }

            geometryContent.Vertices.AddRange(Enumerable.Range(count, aiMesh.VertexCount));
            geometryContent.Indices.AddRange(aiMesh.GetIndices());
            if (aiMesh.HasBones)
            {
                List<BoneWeightCollection> list = new List<BoneWeightCollection>();
                int vertexCount = geometryContent.Vertices.VertexCount;
                bool flag = false;
                for (int i = 0; i < vertexCount; i++)
                {
                    BoneWeightCollection boneWeightCollection = new BoneWeightCollection();
                    for (int j = 0; j < aiMesh.BoneCount; j++)
                    {
                        Bone bone = aiMesh.Bones[j];
                        foreach (VertexWeight vertexWeight in bone.VertexWeights)
                        {
                            if (vertexWeight.VertexID == i)
                            {
                                boneWeightCollection.Add(new BoneWeight(bone.Name, vertexWeight.Weight));
                            }
                        }
                    }

                    if (boneWeightCollection.Count == 0)
                    {
                        flag = true;
                        boneWeightCollection.Add(new BoneWeight(aiMesh.Bones[0].Name, 1f));
                    }

                    list.Add(boneWeightCollection);
                }

                if (flag)
                {
                    _context.Logger.LogWarning(string.Empty, _identity, "No bone weights found for one or more vertices of skinned mesh '{0}'.", aiMesh.Name);
                }

                geometryContent.Vertices.Channels.Add(VertexChannelNames.Weights(0), list);
            }

            if (aiMesh.HasNormals)
            {
                geometryContent.Vertices.Channels.Add(VertexChannelNames.Normal(), aiMesh.Normals.Select(new Func<Vector3D, Vector3>(ToXna)));
            }

            for (int k = 0; k < aiMesh.TextureCoordinateChannelCount; k++)
            {
                geometryContent.Vertices.Channels.Add(VertexChannelNames.TextureCoordinate(k), aiMesh.TextureCoordinateChannels[k].Select(new Func<Vector3D, Vector2>(ToXnaTexCoord)));
            }

            for (int l = 0; l < aiMesh.VertexColorChannelCount; l++)
            {
                geometryContent.Vertices.Channels.Add(VertexChannelNames.Color(l), aiMesh.VertexColorChannels[l].Select(new Func<Color4D, Color>(ToXnaColor)));
            }

            return geometryContent;
        }

        private void FindSkeleton()
        {
            _deformationBones = FindDeformationBones(_scene);
            if (_deformationBones.Count == 0)
            {
                return;
            }

            HashSet<Node> hashSet = new HashSet<Node>();
            foreach (string key in _deformationBones.Keys)
            {
                hashSet.Add(FindRootBone(_scene, key));
            }

            if (hashSet.Count > 1)
            {
                throw new InvalidContentException("Multiple skeletons found. Please ensure that the model does not contain more that one skeleton.", _identity);
            }

            _rootBone = hashSet.First();
            GetSubtree(_rootBone, _bones);
        }

        private static Dictionary<string, Matrix> FindDeformationBones(Scene scene)
        {
            Dictionary<string, Matrix> dictionary = new Dictionary<string, Matrix>();
            if (scene.HasMeshes)
            {
                foreach (Mesh mesh in scene.Meshes)
                {
                    if (mesh.HasBones)
                    {
                        foreach (Bone bone in mesh.Bones)
                        {
                            if (!dictionary.ContainsKey(bone.Name))
                            {
                                dictionary[bone.Name] = ToXna(bone.OffsetMatrix);
                            }
                        }
                    }
                }

                return dictionary;
            }

            return dictionary;
        }

        private static Node FindRootBone(Scene scene, string boneName)
        {
            Node node = scene.RootNode.FindNode(boneName);
            Node result = node;
            while (node != scene.RootNode && !node.HasMeshes)
            {
                if (!node.Name.Contains("$AssimpFbx$"))
                {
                    result = node;
                }

                node = node.Parent;
            }

            return result;
        }

        private void ImportSkeleton()
        {
            if (_rootBone == null)
            {
                return;
            }

            BoneContent boneContent = (BoneContent)ImportBones(_rootBone, _rootBone.Parent, null);
            _rootNode.Children.Add(boneContent);
            if (!_scene.HasAnimations)
            {
                return;
            }

            foreach (Animation animation in _scene.Animations)
            {
                AnimationContent animationContent = ImportAnimation(animation);
                boneContent.Animations.Add(animationContent.Name, animationContent);
            }
        }

        private NodeContent ImportBones(Node aiNode, Node aiParent, NodeContent parent)
        {
            NodeContent nodeContent = null;
            if (!aiNode.Name.Contains("_$AssimpFbx$"))
            {
                if (aiNode.Name.Contains("_$AssimpFbxNull$"))
                {
                    nodeContent = new NodeContent
                    {
                        Name = aiNode.Name.Replace("_$AssimpFbxNull$", string.Empty),
                        Identity = _identity,
                        Transform = ToXna(GetRelativeTransform(aiNode, aiParent))
                    };
                }
                else if (_bones.Contains(aiNode))
                {
                    nodeContent = new BoneContent
                    {
                        Name = aiNode.Name,
                        Identity = _identity
                    };
                    Matrix value;
                    bool flag = _deformationBones.TryGetValue(aiNode.Name, out value);
                    Matrix value2;
                    bool flag2 = _deformationBones.TryGetValue(aiParent.Name, out value2);
                    if (flag && flag2)
                    {
                        nodeContent.Transform = Matrix.Invert(value) * value2;
                    }
                    else if (flag && aiNode == _rootBone)
                    {
                        if (_pivots.TryGetValue(nodeContent.Name, out var value3))
                        {
                            nodeContent.Transform = value3.GetTransform(null, null, null);
                        }
                        else
                        {
                            nodeContent.Transform = Matrix.Invert(value);
                        }
                    }
                    else if (flag && aiParent == _rootBone)
                    {
                        value2 = Matrix.Invert(parent.Transform);
                        nodeContent.Transform = Matrix.Invert(value) * value2;
                    }
                    else
                    {
                        nodeContent.Transform = ToXna(GetRelativeTransform(aiNode, aiParent));
                    }
                }
            }

            if (nodeContent != null)
            {
                parent?.Children.Add(nodeContent);
                aiParent = aiNode;
                parent = nodeContent;
            }

            foreach (Node child in aiNode.Children)
            {
                ImportBones(child, aiParent, parent);
            }

            return nodeContent;
        }

        private AnimationContent ImportAnimation(Animation aiAnimation, string nodeName = null)
        {
            AnimationContent animationContent = new AnimationContent
            {
                Name = GetAnimationName(aiAnimation.Name),
                Identity = _identity,
                Duration = TimeSpan.FromSeconds(aiAnimation.DurationInTicks / aiAnimation.TicksPerSecond)
            };
            IEnumerable<IGrouping<string, NodeAnimationChannel>> enumerable = ((nodeName == null) ? (from channel in aiAnimation.NodeAnimationChannels
                                                                                                     group channel by GetNodeName(channel.NodeName)) : (from channel in aiAnimation.NodeAnimationChannels
                                                                                                                                                        where nodeName == GetNodeName(channel.NodeName)
                                                                                                                                                        group channel by GetNodeName(channel.NodeName)));
            foreach (IGrouping<string, NodeAnimationChannel> item in enumerable)
            {
                string key = item.Key;
                AnimationChannel animationChannel = new AnimationChannel();
                if (!_pivots.TryGetValue(key, out var value))
                {
                    value = FbxPivot.Default;
                }

                List<VectorKey> list = EmptyVectorKeys;
                List<QuaternionKey> list2 = EmptyQuaternionKeys;
                List<VectorKey> list3 = EmptyVectorKeys;
                foreach (NodeAnimationChannel item2 in item)
                {
                    if (item2.NodeName.EndsWith("_$AssimpFbx$_Scaling"))
                    {
                        list = item2.ScalingKeys;
                        continue;
                    }

                    if (item2.NodeName.EndsWith("_$AssimpFbx$_Rotation"))
                    {
                        list2 = item2.RotationKeys;
                        continue;
                    }

                    if (item2.NodeName.EndsWith("_$AssimpFbx$_Translation"))
                    {
                        list3 = item2.PositionKeys;
                        continue;
                    }

                    list = item2.ScalingKeys;
                    list2 = item2.RotationKeys;
                    list3 = item2.PositionKeys;
                }

                List<double> list4 = (from t in list.Select((VectorKey k) => k.Time).Union(list2.Select((QuaternionKey k) => k.Time)).Union(list3.Select((VectorKey k) => k.Time))
                                      orderby t
                                      select t).ToList();
                int num = -1;
                int num2 = -1;
                int num3 = -1;
                double num4 = 0.0;
                double num5 = 0.0;
                double num6 = 0.0;
                Vector3? vector = null;
                Quaternion? quaternion = null;
                Vector3? vector2 = null;
                foreach (double time in list4)
                {
                    int num7 = list.FindIndex((VectorKey k) => k.Time == time);
                    Vector3? vector3;
                    if (num7 != -1)
                    {
                        vector3 = ToXna(list[num7].Value);
                        num = num7;
                        num4 = time;
                        vector = vector3;
                    }
                    else if (num != -1 && num + 1 < list.Count)
                    {
                        VectorKey vectorKey = list[num + 1];
                        double time2 = vectorKey.Time;
                        Vector3 value2 = ToXna(vectorKey.Value);
                        float amount = (float)((time - num4) / (time2 - num4));
                        vector3 = Vector3.Lerp(vector.Value, value2, amount);
                    }
                    else
                    {
                        vector3 = vector;
                    }

                    int num8 = list2.FindIndex((QuaternionKey k) => k.Time == time);
                    Quaternion? quaternion2;
                    if (num8 != -1)
                    {
                        quaternion2 = ToXna(list2[num8].Value);
                        num2 = num8;
                        num5 = time;
                        quaternion = quaternion2;
                    }
                    else if (num2 != -1 && num2 + 1 < list2.Count)
                    {
                        QuaternionKey quaternionKey = list2[num2 + 1];
                        double time3 = quaternionKey.Time;
                        Quaternion quaternion3 = ToXna(quaternionKey.Value);
                        float amount2 = (float)((time - num5) / (time3 - num5));
                        quaternion2 = Quaternion.Slerp(quaternion.Value, quaternion3, amount2);
                    }
                    else
                    {
                        quaternion2 = quaternion;
                    }

                    int num9 = list3.FindIndex((VectorKey k) => k.Time == time);
                    Vector3? vector4;
                    if (num9 != -1)
                    {
                        vector4 = ToXna(list3[num9].Value);
                        num3 = num9;
                        num6 = time;
                        vector2 = vector4;
                    }
                    else if (num3 != -1 && num3 + 1 < list3.Count)
                    {
                        VectorKey vectorKey2 = list3[num3 + 1];
                        double time4 = vectorKey2.Time;
                        Vector3 value3 = ToXna(vectorKey2.Value);
                        float amount3 = (float)((time - num6) / (time4 - num6));
                        vector4 = Vector3.Lerp(vector2.Value, value3, amount3);
                    }
                    else
                    {
                        vector4 = vector2;
                    }

                    Matrix transform = value.GetTransform(vector3, quaternion2, vector4);
                    long value4 = (long)(time * (10000000.0 / aiAnimation.TicksPerSecond));
                    animationChannel.Add(new AnimationKeyframe(TimeSpan.FromTicks(value4), transform));
                }

                animationContent.Channels[item.Key] = animationChannel;
            }

            return animationContent;
        }

        private static void GetSubtree(Node node, List<Node> list)
        {
            list.Add(node);
            foreach (Node child in node.Children)
            {
                GetSubtree(child, list);
            }
        }

        private static Matrix4x4 GetRelativeTransform(Node node, Node ancestor)
        {
            Matrix4x4 transform = node.Transform;
            Node parent = node.Parent;
            while (parent != null && parent != ancestor)
            {
                transform *= parent.Transform;
                parent = parent.Parent;
            }

            if (parent == null && ancestor != null)
            {
                throw new ArgumentException($"Node \"{ancestor.Name}\" is not an ancestor of \"{node.Name}\".");
            }

            return transform;
        }

        private static string GetAnimationName(string name)
        {
            return name.Replace("AnimStack::", string.Empty);
        }

        private static string GetNodeName(string name)
        {
            int num = name.IndexOf("_$AssimpFbx$", StringComparison.Ordinal);
            if (num < 0)
            {
                return name;
            }

            return name.Remove(num);
        }

        [DebuggerStepThrough]
        public static Matrix ToXna(Matrix4x4 matrix)
        {
            Matrix identity = Matrix.Identity;
            identity.M11 = matrix.A1;
            identity.M12 = matrix.B1;
            identity.M13 = matrix.C1;
            identity.M14 = matrix.D1;
            identity.M21 = matrix.A2;
            identity.M22 = matrix.B2;
            identity.M23 = matrix.C2;
            identity.M24 = matrix.D2;
            identity.M31 = matrix.A3;
            identity.M32 = matrix.B3;
            identity.M33 = matrix.C3;
            identity.M34 = matrix.D3;
            identity.M41 = matrix.A4;
            identity.M42 = matrix.B4;
            identity.M43 = matrix.C4;
            identity.M44 = matrix.D4;
            return identity;
        }

        [DebuggerStepThrough]
        public static Vector2 ToXna(Vector2D vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        [DebuggerStepThrough]
        public static Vector3 ToXna(Vector3D vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }

        [DebuggerStepThrough]
        public static Quaternion ToXna(Assimp.Quaternion quaternion)
        {
            return new Quaternion(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W);
        }

        [DebuggerStepThrough]
        public static Vector3 ToXna(Color4D color)
        {
            return new Vector3(color.R, color.G, color.B);
        }

        [DebuggerStepThrough]
        public static Vector2 ToXnaTexCoord(Vector3D vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        [DebuggerStepThrough]
        public static Color ToXnaColor(Color4D color)
        {
            return new Color(color.R, color.G, color.B, color.A);
        }

    }
}
