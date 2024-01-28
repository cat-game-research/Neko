using UnityEngine;
using System.Collections.Generic;
using CW.Common;

namespace SpaceGraphicsToolkit.Aurora
{
	/// <summary>This component allows you to render an aurora above a planet. The aurora can be set to procedurally animate in the shader.</summary>
	[ExecuteInEditMode]
	[HelpURL(SgtCommon.HelpUrlPrefix + "SgtAurora")]
	[AddComponentMenu(SgtCommon.ComponentMenuPrefix + "Aurora")]
	public class SgtAurora : MonoBehaviour, CwChild.IHasChildren
	{
		/// <summary>The material used to render this component.
		/// NOTE: This material must use the <b>Space Graphics Toolkit/Aurora</b> shader. You cannot use a normal shader.</summary>
		public Material SourceMaterial { set { if (sourceMaterial != value) { sourceMaterial = value; } } get { return sourceMaterial; } } [SerializeField] private Material sourceMaterial;

		/// <summary>This allows you to set the overall color of the aurora.</summary>
		public Color Color { set { color = value; } get { return color; } } [SerializeField] private Color color = Color.white;

		/// <summary>The <b>Color.rgb</b> values will be multiplied by this.</summary>
		public float Brightness { set { brightness = value; } get { return brightness; } } [SerializeField] private float brightness = 1.0f;

		/// <summary>This allows you to offset the camera distance in world space when rendering the aurora, giving you fine control over the render order.</summary>
		public float CameraOffset { set { cameraOffset = value; } get { return cameraOffset; } } [SerializeField] private float cameraOffset;

		/// <summary>This allows you to set the random seed used during procedural generation.</summary>
		public int Seed { set { if (seed != value) { seed = value; dirtyMesh = true; } } get { return seed; } } [SerializeField] [CwSeed] private int seed;

		/// <summary>The inner radius of the aurora mesh in local space.</summary>
		public float RadiusMin { set { radiusMin = value; } get { return radiusMin; } } [SerializeField] private float radiusMin = 1.0f;

		/// <summary>The inner radius of the aurora mesh in local space.</summary>
		public float RadiusMax { set { radiusMax = value; } get { return radiusMax; } } [SerializeField] private float radiusMax = 1.1f;

		/// <summary>The amount of aurora paths/ribbons.</summary>
		public int PathCount { set { if (pathCount != value) { pathCount = value; dirtyMesh = true; } } get { return pathCount; } } [SerializeField] private int pathCount = 8;

		/// <summary>The amount of quads used to build each path.</summary>
		public int PathDetail { set { if (pathDetail != value) { pathDetail = value; dirtyMesh = true; } } get { return pathDetail; } } [SerializeField] private int pathDetail = 100;

		/// <summary>The minimum length of each aurora path.</summary>
		public float PathLengthMin { set { if (pathLengthMin != value) { pathLengthMin = value; dirtyMesh = true; } } get { return pathLengthMin; } } [SerializeField] [Range(0.0f, 1.0f)] private float pathLengthMin = 0.1f;

		/// <summary>The maximum length of each aurora path.</summary>
		public float PathLengthMax { set { if (pathLengthMax != value) { pathLengthMax = value; dirtyMesh = true; } } get { return pathLengthMax; } } [SerializeField] [Range(0.0f, 1.0f)] private float pathLengthMax = 0.1f;

		/// <summary>The minimum distance between the pole and the aurora path start point.</summary>
		public float StartMin { set { if (startMin != value) { startMin = value; dirtyMesh = true; } } get { return startMin; } } [SerializeField] [Range(0.0f, 1.0f)] private float startMin = 0.1f;

		/// <summary>The maximum distance between the pole and the aurora path start point.</summary>
		public float StartMax { set { if (startMax != value) { startMax = value; dirtyMesh = true; } } get { return startMax; } } [SerializeField] [Range(0.0f, 1.0f)] private float startMax = 0.5f;

		/// <summary>The probability that the aurora path will begin closer to the pole.</summary>
		public float StartBias { set { if (startBias != value) { startBias = value; dirtyMesh = true; } } get { return startBias; } } [SerializeField] private float startBias = 1.0f;

		/// <summary>The probability that the aurora path will start on the northern pole.</summary>
		public float StartTop { set { if (startTop != value) { startTop = value; dirtyMesh = true; } } get { return startTop; } } [SerializeField] [Range(0.0f, 1.0f)] private float startTop = 0.5f;

		/// <summary>The amount of waypoints the aurora path will follow based on its length.</summary>
		public int PointDetail { set { if (pointDetail != value) { pointDetail = value; dirtyMesh = true; } } get { return pointDetail; } } [SerializeField] [Range(1, 100)] private int pointDetail = 10;

		/// <summary>The strength of the aurora waypoint twisting.</summary>
		public float PointSpiral { set { if (pointSpiral != value) { pointSpiral = value; dirtyMesh = true; } } get { return pointSpiral; } } [SerializeField] private float pointSpiral = 1.0f;

		/// <summary>The strength of the aurora waypoint random displacement.</summary>
		public float PointJitter { set { if (pointJitter != value) { pointJitter = value; dirtyMesh = true; } } get { return pointJitter; } } [SerializeField] [Range(0.0f, 1.0f)] private float pointJitter = 1.0f;

		/// <summary>The sharpness of the fading at the start and ends of the aurora paths.</summary>
		public float TrailEdgeFade { set { if (trailEdgeFade != value) { trailEdgeFade = value; dirtyMesh = true; } } get { return trailEdgeFade; } } [SerializeField] private float trailEdgeFade = 1.0f;

		/// <summary>The amount of times the main texture is tiled based on its length.</summary>
		public float TrailTile { set { if (trailTile != value) { trailTile = value; dirtyMesh = true; } } get { return trailTile; } } [SerializeField] private float trailTile = 30.0f;

		/// <summary>The flatness of the aurora path.</summary>
		public float TrailHeights { set { if (trailHeights != value) { trailHeights = value; dirtyMesh = true; } } get { return trailHeights; } } [SerializeField] [Range(0.1f, 1.0f)] private float trailHeights = 1.0f;

		/// <summary>The amount of height changes in the aurora path.</summary>
		public int TrailHeightsDetail { set { if (trailHeightsDetail != value) { trailHeightsDetail = value; dirtyMesh = true; } } get { return trailHeightsDetail; } } [SerializeField] private int trailHeightsDetail = 10;

		/// <summary>The possible colors given to the top half of the aurora path.</summary>
		public Gradient Colors { get { if (colors == null) colors = new Gradient(); return colors; } } [SerializeField] private Gradient colors;

		/// <summary>The amount of color changes an aurora path can have based on its length.</summary>
		public int ColorsDetail { set { if (colorsDetail != value) { colorsDetail = value; dirtyMesh = true; } } get { return colorsDetail; } } [SerializeField] private int colorsDetail = 10;

		/// <summary>The minimum opacity multiplier of the aurora path colors.</summary>
		public float ColorsAlpha { set { if (colorsAlpha != value) { colorsAlpha = value; dirtyMesh = true; } } get { return colorsAlpha; } } [SerializeField] [Range(0.0f, 1.0f)] private float colorsAlpha = 0.5f;

		/// <summary>The amount of alpha changes in the aurora path.</summary>
		public float ColorsAlphaBias { set { if (colorsAlphaBias != value) { colorsAlphaBias = value; dirtyMesh = true; } } get { return colorsAlphaBias; } } [SerializeField] private float colorsAlphaBias = 2.0f;

		/// <summary>The strength of the aurora path position changes in local space.</summary>
		public float AnimStrength { set { if (animStrength != value) { animStrength = value; dirtyMesh = true; } } get { return animStrength; } } [SerializeField] private float animStrength = 0.01f;

		/// <summary>The amount of the animation strength changes along the aurora path based on its length.</summary>
		public int AnimStrengthDetail { set { if (animStrengthDetail != value) { animStrengthDetail = value; dirtyMesh = true; } } get { return animStrengthDetail; } } [SerializeField] private int animStrengthDetail = 10;

		/// <summary>The maximum angle step between sections of the aurora path.</summary>
		public float AnimAngle { set { if (animAngle != value) { animAngle = value; dirtyMesh = true; } } get { return animAngle; } } [SerializeField] private float animAngle = 0.01f;

		/// <summary>The amount of the animation angle changes along the aurora path based on its length.</summary>
		public int AnimAngleDetail { set { if (animAngleDetail != value) { animAngleDetail = value; dirtyMesh = true; } } get { return animAngleDetail; } } [SerializeField] private int animAngleDetail = 10;

		public event System.Action<Material> OnSetProperties;

		[SerializeField]
		private SgtAuroraModel model;
		
		[System.NonSerialized]
		private Mesh mesh;

		[System.NonSerialized]
		private Material material;

		[System.NonSerialized]
		private bool dirtyMesh = true;

		[System.NonSerialized]
		private Transform cachedTransform;

		private static List<Vector3> tempPositions = new List<Vector3>();
		private static List<Vector4> tempCoords0   = new List<Vector4>();
		private static List<Color>   tempColors    = new List<Color>();
		private static List<Vector3> tempNormals   = new List<Vector3>();
		private static List<int>     tempIndices   = new List<int>();
		
		private static int _SGT_MainTex    = Shader.PropertyToID("_SGT_MainTex");
		private static int _SGT_NearTex    = Shader.PropertyToID("_SGT_NearTex");
		private static int _SGT_Color      = Shader.PropertyToID("_SGT_Color");
		private static int _SGT_Brightness = Shader.PropertyToID("_SGT_Brightness");
		private static int _SGT_RadiusMin  = Shader.PropertyToID("_SGT_RadiusMin");
		private static int _SGT_RadiusSize = Shader.PropertyToID("_SGT_RadiusSize");

		public SgtAuroraModel Model
		{
			get
			{
				return model;
			}
		}

		public Material Material
		{
			get
			{
				return material;
			}
		}

		public bool NeedsMainTex
		{
			get
			{
				if (material != null && material.GetTexture(_SGT_MainTex) == null)
				{
					return true;
				}

				return false;
			}
		}

		public bool NeedsNearTex
		{
			get
			{
				if (material != null && material.IsKeywordEnabled("_SGT_NEAR") == true && material.GetTexture(_SGT_NearTex) == null)
				{
					return true;
				}

				return false;
			}
		}

		public bool HasChild(CwChild child)
		{
			return child == model;
		}

		private void HandleCameraPreRender(Camera camera)
		{
			if (sourceMaterial != null)
			{
				if (material == null)
				{
					material = CwHelper.CreateTempMaterial("Belt (Generated)", sourceMaterial);

					if (model != null)
					{
						model.CachedMeshRenderer.sharedMaterial = material;
					}
				}

				if (OnSetProperties != null)
				{
					OnSetProperties.Invoke(material);
				}
				
				material.SetColor(_SGT_Color, color);
				material.SetFloat(_SGT_Brightness, brightness);
				material.SetFloat(_SGT_RadiusMin, radiusMin);
				material.SetFloat(_SGT_RadiusSize, radiusMax - radiusMin);

				if (cameraOffset != 0.0f)
				{
					var direction = Vector3.Normalize(camera.transform.position - cachedTransform.position);

					model.transform.position = cachedTransform.position + direction * cameraOffset;
				}
				else
				{
					model.transform.localPosition = Vector3.zero;
				}
			}
		}

		private void BakeMesh(Mesh mesh)
		{
			mesh.Clear(false);
			mesh.SetVertices(tempPositions);
			mesh.SetUVs(0, tempCoords0);
			mesh.SetColors(tempColors);
			mesh.SetNormals(tempNormals);
			mesh.SetTriangles(tempIndices, 0);

			mesh.bounds = new Bounds(Vector3.zero, Vector3.one * radiusMax * 2.0f);
		}

		private Vector3 GetStart(float angle)
		{
			var distance = Mathf.Lerp(startMin, startMax, Mathf.Pow(Random.value, startBias));

			if (Random.value < startTop)
			{
				return new Vector3(Mathf.Sin(angle) * distance, 1.0f, Mathf.Cos(angle) * distance);
			}
			else
			{
				return new Vector3(Mathf.Sin(angle) * distance, -1.0f, Mathf.Cos(angle) * distance);
			}
		}

		private Vector3 GetNext(Vector3 point, float angle, float speed)
		{
			var noise = Random.insideUnitCircle;

			point.x += Mathf.Sin(angle) * speed;
			point.z += Mathf.Cos(angle) * speed;

			point.x += noise.x * pointJitter;
			point.z += noise.y * pointJitter;

			return Quaternion.Euler(0.0f, pointSpiral, 0.0f) * point;
		}

		private float GetNextAngle(float angle)
		{
			return angle + Random.Range(0.0f, animAngle);
		}

		private float GetNextStrength()
		{
			return Random.Range(-animStrength, animStrength);
		}

		private Color GetNextColor()
		{
			var color = Color.white;

			if (Colors != null)
			{
				color = Colors.Evaluate(Random.value);
			}

			color.a *= Mathf.LerpUnclamped(colorsAlpha, 1.0f, Mathf.Pow(Random.value, colorsAlphaBias));

			return color;
		}

		private float GetNextHeight()
		{
			return Random.Range(0.0f, trailHeights);
		}

		private void Shift<T>(ref T a, ref T b, ref T c, T d, ref float f)
		{
			a  = b;
			b  = c;
			c  = d;
			f -= 1.0f;
		}

		private void AddPath(Mesh mesh, ref int vertexCount)
		{
			var pathLength = Random.Range(pathLengthMin, pathLengthMax);
			var lineCount  = 2 + (int)(pathLength * pathDetail);
			var quadCount  = lineCount - 1;
			var vertices   = quadCount * 2 + 2;

			var angle      = Random.Range(-Mathf.PI, Mathf.PI);
			var speed      = 1.0f / pointDetail;
			var detailStep = 1.0f / pathDetail;
			var pointStep  = detailStep * pointDetail;
			var pointFrac  = 0.0f;
			var pointA     = GetStart(angle + Mathf.PI);
			var pointB     = GetNext(pointA, angle, speed);
			var pointC     = GetNext(pointB, angle, speed);
			var pointD     = GetNext(pointC, angle, speed);
			var coordFrac  = 0.0f;
			var edgeFrac   = -1.0f;
			var edgeStep   = 2.0f / lineCount;
			var coordStep  = detailStep * trailTile;

			var angleA = angle;
			var angleB = GetNextAngle(angleA);
			var angleC = GetNextAngle(angleB);
			var angleD = GetNextAngle(angleC);
			var angleFrac = 0.0f;
			var angleStep = detailStep * animAngleDetail;

			var strengthA    = 0.0f;
			var strengthB    = GetNextStrength();
			var strengthC    = GetNextStrength();
			var strengthD    = GetNextStrength();
			var strengthFrac = 0.0f;
			var strengthStep = detailStep * animStrengthDetail;

			var colorA    = GetNextColor();
			var colorB    = GetNextColor();
			var colorC    = GetNextColor();
			var colorD    = GetNextColor();
			var colorFrac = 0.0f;
			var colorStep = detailStep * colorsDetail;

			var heightA    = GetNextHeight();
			var heightB    = GetNextHeight();
			var heightC    = GetNextHeight();
			var heightD    = GetNextHeight();
			var heightFrac = 0.0f;
			var heightStep = detailStep * trailHeightsDetail;

			for (var i = 0; i < lineCount; i++)
			{
				while (pointFrac >= 1.0f)
				{
					Shift(ref pointA, ref pointB, ref pointC, pointD, ref pointFrac); pointD = GetNext(pointC, angle, speed);
				}

				while (angleFrac >= 1.0f)
				{
					Shift(ref angleA, ref angleB, ref angleC, angleD, ref angleFrac); angleD = GetNextAngle(angleC);
				}

				while (strengthFrac >= 1.0f)
				{
					Shift(ref strengthA, ref strengthB, ref strengthC, strengthD, ref strengthFrac); strengthD = GetNextStrength();
				}

				while (colorFrac >= 1.0f)
				{
					Shift(ref colorA, ref colorB, ref colorC, colorD, ref colorFrac); colorD = GetNextColor();
				}

				while (heightFrac >= 1.0f)
				{
					Shift(ref heightA, ref heightB, ref heightC, heightD, ref heightFrac); heightD = GetNextHeight();
				}

				var point   = SgtCommon.HermiteInterpolate3(pointA, pointB, pointC, pointD, pointFrac);
				var animAng = SgtCommon.HermiteInterpolate(angleA, angleB, angleC, angleD, angleFrac);
				var animStr = SgtCommon.HermiteInterpolate(strengthA, strengthB, strengthC, strengthD, strengthFrac);
				var color   = SgtCommon.HermiteInterpolate(colorA, colorB, colorC, colorD, colorFrac);
				var height  = SgtCommon.HermiteInterpolate(heightA, heightB, heightC, heightD, heightFrac);

				// Fade edges
				color.a *= Mathf.SmoothStep(1.0f, 0.0f, Mathf.Pow(Mathf.Abs(edgeFrac), trailEdgeFade));

				tempCoords0.Add(new Vector4(coordFrac, 0.0f, animAng, animStr));
				tempCoords0.Add(new Vector4(coordFrac, height, animAng, animStr));

				tempPositions.Add(point);
				tempPositions.Add(point);

				tempColors.Add(color);
				tempColors.Add(color);

				pointFrac    += pointStep;
				edgeFrac     += edgeStep;
				coordFrac    += coordStep;
				angleFrac    += angleStep;
				strengthFrac += strengthStep;
				colorFrac    += colorStep;
				heightFrac   += heightStep;
			}

			var vector = tempPositions[1] - tempPositions[0];

			tempNormals.Add(GetNormal(vector, vector));
			tempNormals.Add(GetNormal(vector, vector));

			for (var i = 2; i < lineCount; i++)
			{
				var nextVector = tempPositions[i] - tempPositions[i - 1];

				tempNormals.Add(GetNormal(vector, nextVector));
				tempNormals.Add(GetNormal(vector, nextVector));

				vector = nextVector;
			}

			tempNormals.Add(GetNormal(vector, vector));
			tempNormals.Add(GetNormal(vector, vector));

			for (var i = 0; i < quadCount; i++)
			{
				var offset = vertexCount + i * 2;

				tempIndices.Add(offset + 0);
				tempIndices.Add(offset + 1);
				tempIndices.Add(offset + 2);

				tempIndices.Add(offset + 3);
				tempIndices.Add(offset + 2);
				tempIndices.Add(offset + 1);
			}

			vertexCount += vertices;
		}

		private Vector3 GetNormal(Vector3 a, Vector3 b)
		{
			return Vector3.Cross(a.normalized, b.normalized);
		}

		private void UpdateMesh()
		{
			if (mesh == null)
			{
				mesh = SgtCommon.CreateTempMesh("Aurora Mesh (Generated)");
			}

			SgtCommon.ClearCapacity(tempPositions, 1024);
			SgtCommon.ClearCapacity(tempCoords0, 1024);
			SgtCommon.ClearCapacity(tempIndices, 1024);
			SgtCommon.ClearCapacity(tempColors, 1024);
			SgtCommon.ClearCapacity(tempNormals, 1024);

			if (pathDetail > 0 && pathLengthMin > 0.0f && pathLengthMax > 0.0f)
			{
				var vertexCount = 0;

				CwHelper.BeginSeed(seed);
				{
					for (var i = 0; i < pathCount; i++)
					{
						AddPath(mesh, ref vertexCount);
					}
				}
				CwHelper.EndSeed();
			}

			BakeMesh(mesh);
		}

		public static SgtAurora Create(int layer = 0, Transform parent = null)
		{
			return Create(layer, parent, Vector3.zero, Quaternion.identity, Vector3.one);
		}

		public static SgtAurora Create(int layer, Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
		{
			return CwHelper.CreateGameObject("Aurora", layer, parent, localPosition, localRotation, localScale).AddComponent<SgtAurora>();
		}

		[ContextMenu("Sync Material")]
		public void SyncMaterial()
		{
			if (sourceMaterial != null)
			{
				if (material != null)
				{
					if (material.shader != sourceMaterial.shader)
					{
						material.shader = sourceMaterial.shader;
					}

					material.CopyPropertiesFromMaterial(sourceMaterial);

					if (OnSetProperties != null)
					{
						OnSetProperties.Invoke(material);
					}
				}
			}
		}

#if UNITY_EDITOR
		protected virtual void Reset()
		{
			if (colors == null)
			{
				colors = new Gradient();

				colors.colorKeys = new GradientColorKey[] { new GradientColorKey(Color.blue, 0.0f), new GradientColorKey(Color.magenta, 1.0f) };
			}
		}
#endif

		protected virtual void OnEnable()
		{
			CwHelper.OnCameraPreRender += HandleCameraPreRender;

			if (model == null)
			{
				model = SgtAuroraModel.Create(this);
			}

			model.CachedMeshRenderer.enabled = true;

			cachedTransform = GetComponent<Transform>();
		}

		protected virtual void OnDisable()
		{
			CwHelper.OnCameraPreRender -= HandleCameraPreRender;

			if (model != null)
			{
				model.CachedMeshRenderer.enabled = false;
			}
		}

		protected virtual void OnDestroy()
		{
			CwHelper.Destroy(mesh);
		}

		protected virtual void LateUpdate()
		{
#if UNITY_EDITOR
			SyncMaterial();
#endif
			if (dirtyMesh == true)
			{
				dirtyMesh = false; UpdateMesh();
			}

			if (model != null)
			{
				model.CachedMeshFilter.sharedMesh = mesh;
			}
		}

		protected virtual void OnDidApplyAnimationProperties()
		{
			dirtyMesh = true;
		}

#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			dirtyMesh = true;
		}
#endif

#if UNITY_EDITOR
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = transform.localToWorldMatrix;

			Gizmos.DrawWireSphere(Vector3.zero, radiusMin);

			Gizmos.DrawWireSphere(Vector3.zero, radiusMax);
		}
#endif
	}
}

#if UNITY_EDITOR
namespace SpaceGraphicsToolkit.Aurora
{
	using UnityEditor;
	using TARGET = SgtAurora;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SgtAurora_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			BeginError(Any(tgts, t => t.SourceMaterial == null));
				Draw("sourceMaterial", "The material used to render this component.\n\nNOTE: This material must use the <b>Space Graphics Toolkit/Aurora</b> shader. You cannot use a normal shader.");
			EndError();
			Draw("color", "This allows you to set the overall color of the aurora.");
			Draw("brightness", "The <b>Color.rgb</b> values will be multiplied by this.");
			Draw("cameraOffset", "This allows you to offset the camera distance in world space when rendering the aurora, giving you fine control over the render order."); // Updated automatically

			Separator();

			Draw("seed", "This allows you to set the random seed used during procedural generation.");
			BeginError(Any(tgts, t => t.RadiusMin >= t.RadiusMax));
				Draw("radiusMin", "The inner radius of the aurora mesh in local space.");
				Draw("radiusMax", "The outer radius of the aurora mesh in local space.");
			EndError();

			Separator();

			BeginError(Any(tgts, t => t.PathCount < 1));
				Draw("pathCount", "The amount of aurora paths/ribbons.");
			EndError();
			BeginError(Any(tgts, t => t.PathDetail < 1));
				Draw("pathDetail", "The amount of quads used to build each path.");
			EndError();
			BeginError(Any(tgts, t => t.PathLengthMin > t.PathLengthMax));
				Draw("pathLengthMin", "The minimum length of each aurora path.");
				Draw("pathLengthMax", "The maximum length of each aurora path.");
			EndError();

			Separator();

			BeginError(Any(tgts, t => t.StartMin > t.StartMax));
				Draw("startMin", "The minimum distance between the pole and the aurora path start point.");
				Draw("startMax", "The maximum distance between the pole and the aurora path start point.");
			EndError();
			BeginError(Any(tgts, t => t.StartBias < 1.0f));
				Draw("startBias", "The probability that the aurora path will begin closer to the pole.");
			EndError();
			Draw("startTop", "The probability that the aurora path will start on the northern pole.");

			Separator();

			Draw("pointDetail", "The amount of waypoints the aurora path will follow based on its length.");
			Draw("pointSpiral", "The strength of the aurora waypoint twisting.");
			Draw("pointJitter", "The strength of the aurora waypoint random displacement.");

			Separator();

			Draw("trailTile", "The amount of times the main texture is tiled based on its length.");
			BeginError(Any(tgts, t => t.TrailEdgeFade < 1.0f));
				Draw("trailEdgeFade", "The sharpness of the fading at the start and ends of the aurora paths.");
			EndError();
			Draw("trailHeights", "The flatness of the aurora path.");
			BeginError(Any(tgts, t => t.TrailHeightsDetail < 1));
				Draw("trailHeightsDetail", "The amount of height changes in the aurora path.");
			EndError();

			Separator();

			Draw("colors", "The possible colors given to the top half of the aurora path.");
			BeginError(Any(tgts, t => t.ColorsDetail < 1));
				Draw("colorsDetail", "The amount of color changes an aurora path can have based on its length.");
			EndError();
			Draw("colorsAlpha", "The minimum opacity multiplier of the aurora path colors.");
			Draw("colorsAlphaBias", "The amount of alpha changes in the aurora path.");

			if (Any(tgts, t => t.NeedsMainTex == true && t.GetComponent<SgtAuroraMainTex>() == null))
			{
				Separator();

				if (HelpButton("SourceMaterial doesn't contain a MainTex.", MessageType.Error, "Fix", 30) == true)
				{
					Each(tgts, t => CwHelper.GetOrAddComponent<SgtAuroraMainTex>(t.gameObject));
				}
			}

			if (Any(tgts, t => t.NeedsNearTex == true && t.GetComponent<SgtAuroraNearTex>() == null))
			{
				Separator();

				if (HelpButton("SourceMaterial doesn't contain a NearTex.", MessageType.Error, "Fix", 30) == true)
				{
					Each(tgts, t => CwHelper.GetOrAddComponent<SgtAuroraNearTex>(t.gameObject));
				}
			}
		}

		[MenuItem(SgtCommon.GameObjectMenuPrefix + "Aurora", false, 10)]
		public static void CreateMenuItem()
		{
			var parent   = CwHelper.GetSelectedParent();
			var instance = SgtAurora.Create(parent != null ? parent.gameObject.layer : 0, parent);

			CwHelper.SelectAndPing(instance);
		}
	}
}
#endif