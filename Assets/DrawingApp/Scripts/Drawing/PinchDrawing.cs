using UnityEngine;
using System.Collections.Generic;
using Leap.Unity;
using System.IO;
using System.Text;
using UnityEngine.Events;

[System.Serializable]
public class StringEvent : UnityEvent<string>  { }

public class PinchDrawing : MonoBehaviour {

  #region PUBLIC FIELDS

  [Tooltip("When a pinch is detected, this behaviour will initiate drawing a tube at the pinch location.")]
  public PinchDetector[] _pinchDetectors;

  [Tooltip("Before starting drawing, this detector will be checked. If it is active, drawing won't actually begin.")]
  public Detector _dontDrawDetector;

  [Tooltip("When drawing properties are changed, a preview tube is updated to reflect those changes. "
         + "The preview will be drawn centered at the root of this transform.")]
  public Transform _previewStrokeTransform;

  [Tooltip("Draw the preview stroke using the current drawing settings at the preview stroke transform.")]
  public bool _displayPreviewStroke = false;

  [Header("Drawing")]
  
  [Tooltip("The color of newly drawn tubes.")]
  public Color _tubeColor = Color.black;

  [Tooltip("The material to assign to newly drawn tubes.")]
  public Material _tubeMaterial;

  [Tooltip("The thickness of newly drawn tubes.")]
  [Range(0.001F, 0.01F)]
  public float _tubeRadius = 0.002F;

  [Tooltip("Higher values require longer strokes but make the strokes smoother.")]
  private float _tubeSmoothingDelay = 0.01F;

  [Tooltip("The number of vertices per ring of newly drawn tubes.")]
  public int _tubeResolution = 8;

  [Tooltip("Tubes are drawn in segments; this is the minimum length of a segment before it is added to the tube currently being drawn.")]
  public float _tubeMinSegmentLength = 0.005F;

  #endregion PUBLIC ATTRIBUTES

  #region PRIVATE FIELDS

  #region OLD MULTIPLE STATE CODE

  /// <summary>
  /// TODO DELETEME
  /// DrawState objects each process beginning, creating,
  /// and ending the meshes from input lines as a series of
  /// Vector3s.
  /// 
  /// Predefined index assumptions:
  /// [0] Right Hand;
  /// [1] Left Hand (currently NYI)
  /// </summary>
  //private DrawState[] _drawStates;

  /// <summary>
  /// TODO DELETE
  /// Stores a list of the tubes created per DrawState.
  /// Upon Undo(), the last-added GameObject is removed
  /// and Destroy()'d.
  /// </summary>
  //private List<GameObject>[] _undoHistories;

  #endregion

  /// <summary>
  /// DrawState objects process beginning, creating, and
  /// finalizing the meshes from input lines as a series of
  /// Vector3s.
  /// </summary>
  private DrawState _drawState;

  /// <summary>
  /// A growable list of GameObjects holding all of the objects
  /// containing drawn meshes in the order in which they were
  /// drawn.
  /// </summary>
  private List<GameObject> _undoHistory;

  /// <summary>
  /// As the user performs Undo operations, "undone" objects
  /// are moved into Redo history to faciliate undo-ing undo operations.
  /// 
  /// This buffer is cleared whenever the user makes any new strokes
  /// (and its objects are finally deleted).
  /// </summary>
  private List<GameObject> _redoHistory;

  /// <summary>
  /// A DrawState used in the UI to draw preview tubes
  /// with the current tube settings.
  /// </summary>
  private DrawState _previewDrawState;

  /// <summary>
  /// The GameObject previewing a tube created with a predefined stroke
  /// under the current tube settings.
  /// </summary>
  private GameObject _previewTube;

  /// <summary>
  /// Stroke data to generate a stroke preview on the UI.
  /// </summary>
  private Vector3[] _previewStrokePoints = new Vector3[] {
    #region Preview Stroke Point Data
new Vector3(-0.226124F, 1.48719F, 0.0955172F),
new Vector3(-0.223764F, 1.48636F, 0.0943602F),
new Vector3(-0.222229F, 1.48683F, 0.0927576F),
new Vector3(-0.219338F, 1.48612F, 0.0907258F),
new Vector3(-0.217506F, 1.48495F, 0.0897889F),
new Vector3(-0.214241F, 1.48369F, 0.0880756F),
new Vector3(-0.211155F, 1.48387F, 0.0865588F),
new Vector3(-0.207456F, 1.48359F, 0.0853735F),
new Vector3(-0.204851F, 1.48322F, 0.084379F),
new Vector3(-0.201316F, 1.4842F, 0.0835621F),
new Vector3(-0.19756F, 1.48465F, 0.0817086F),
new Vector3(-0.193886F, 1.48531F, 0.0798718F),
new Vector3(-0.19057F, 1.48524F, 0.0783217F),
new Vector3(-0.185253F, 1.48341F, 0.0773396F),
new Vector3(-0.184439F, 1.48308F, 0.0773598F),
new Vector3(-0.182815F, 1.48248F, 0.0748541F),
new Vector3(-0.180238F, 1.48124F, 0.0746258F),
new Vector3(-0.177615F, 1.47913F, 0.075653F),
new Vector3(-0.174806F, 1.47808F, 0.0748577F),
new Vector3(-0.170791F, 1.47746F, 0.0721869F),
new Vector3(-0.167678F, 1.47655F, 0.0727501F),
new Vector3(-0.165302F, 1.47419F, 0.0738672F),
new Vector3(-0.161997F, 1.47316F, 0.0733727F),
new Vector3(-0.15869F, 1.47313F, 0.0708133F),
new Vector3(-0.155924F, 1.47134F, 0.070856F),
new Vector3(-0.153676F, 1.46776F, 0.0727187F),
new Vector3(-0.150103F, 1.46535F, 0.0732156F),
new Vector3(-0.145538F, 1.46212F, 0.0738737F),
new Vector3(-0.143516F, 1.46222F, 0.0723969F),
new Vector3(-0.140694F, 1.45987F, 0.0726059F),
new Vector3(-0.138107F, 1.45692F, 0.0738742F),
new Vector3(-0.135371F, 1.45443F, 0.0735954F),
new Vector3(-0.133643F, 1.45283F, 0.0713758F),
new Vector3(-0.131241F, 1.45039F, 0.0720074F),
new Vector3(-0.127894F, 1.44695F, 0.0739909F),
new Vector3(-0.12556F, 1.44495F, 0.073827F),
new Vector3(-0.124128F, 1.44409F, 0.0725236F),
new Vector3(-0.12135F, 1.44221F, 0.0734943F),
new Vector3(-0.118997F, 1.43967F, 0.0736431F),
new Vector3(-0.116477F, 1.43703F, 0.0741226F),
new Vector3(-0.115155F, 1.43547F, 0.0747532F),
new Vector3(-0.113312F, 1.43351F, 0.0744839F),
new Vector3(-0.111281F, 1.43125F, 0.0755033F),
new Vector3(-0.109516F, 1.42908F, 0.0764308F),
new Vector3(-0.107761F, 1.42708F, 0.0768306F),
new Vector3(-0.106254F, 1.42599F, 0.0769699F),
new Vector3(-0.104939F, 1.42376F, 0.0773063F),
new Vector3(-0.10272F, 1.4212F, 0.0783273F),
new Vector3(-0.102154F, 1.41986F, 0.0780988F),
new Vector3(-0.102045F, 1.41901F, 0.0769595F),
new Vector3(-0.100883F, 1.41753F, 0.0775486F),
new Vector3(-0.0998258F, 1.41626F, 0.0782439F),
new Vector3(-0.0988289F, 1.41518F, 0.0789278F),
new Vector3(-0.0984394F, 1.41474F, 0.0790367F),
new Vector3(-0.0986539F, 1.41434F, 0.0786585F),
new Vector3(-0.0982774F, 1.41443F, 0.0787254F),
new Vector3(-0.0972868F, 1.41428F, 0.0793952F),
new Vector3(-0.0972813F, 1.41494F, 0.0792384F),
new Vector3(-0.0978155F, 1.41659F, 0.0789379F),
new Vector3(-0.0978268F, 1.41662F, 0.0789338F),
new Vector3(-0.0969985F, 1.41754F, 0.0801245F),
new Vector3(-0.0971161F, 1.4197F, 0.0806696F),
new Vector3(-0.0979699F, 1.4218F, 0.0801851F),
new Vector3(-0.0982955F, 1.42538F, 0.0812063F),
new Vector3(-0.0983464F, 1.42714F, 0.0827001F),
new Vector3(-0.0974866F, 1.4304F, 0.0847738F),
new Vector3(-0.0971836F, 1.43475F, 0.0865783F),
new Vector3(-0.0977473F, 1.43889F, 0.0863155F),
new Vector3(-0.0966364F, 1.44289F, 0.0883293F),
new Vector3(-0.0953628F, 1.44566F, 0.0911542F),
new Vector3(-0.094796F, 1.45053F, 0.0932763F),
new Vector3(-0.0953547F, 1.45471F, 0.0932902F),
new Vector3(-0.0947717F, 1.45949F, 0.0952532F),
new Vector3(-0.092982F, 1.4641F, 0.0993231F),
new Vector3(-0.0920939F, 1.46976F, 0.101466F),
new Vector3(-0.0883518F, 1.47856F, 0.107846F),
new Vector3(-0.0916079F, 1.47807F, 0.103172F),
new Vector3(-0.0891389F, 1.48275F, 0.107128F),
new Vector3(-0.088661F, 1.48822F, 0.107875F),
new Vector3(-0.0878806F, 1.4929F, 0.109547F),
new Vector3(-0.0879876F, 1.49586F, 0.110232F),
new Vector3(-0.0868675F, 1.50074F, 0.112824F),
new Vector3(-0.0838195F, 1.50608F, 0.116914F),
new Vector3(-0.0833457F, 1.5107F, 0.118425F),
new Vector3(-0.0841543F, 1.51349F, 0.119584F),
new Vector3(-0.0826171F, 1.51774F, 0.122778F),
new Vector3(-0.0796182F, 1.52285F, 0.124227F),
new Vector3(-0.0782937F, 1.52713F, 0.125822F),
new Vector3(-0.078652F, 1.52923F, 0.130751F),
new Vector3(-0.0779682F, 1.53102F, 0.131107F),
new Vector3(-0.0752266F, 1.53363F, 0.133971F),
new Vector3(-0.0738156F, 1.53636F, 0.136588F),
new Vector3(-0.073306F, 1.53848F, 0.139098F),
new Vector3(-0.0740191F, 1.53936F, 0.13991F),
new Vector3(-0.0730493F, 1.54114F, 0.142102F),
new Vector3(-0.071277F, 1.54341F, 0.144481F),
new Vector3(-0.0707207F, 1.54476F, 0.146152F),
new Vector3(-0.0713428F, 1.54542F, 0.146692F),
new Vector3(-0.0709478F, 1.54704F, 0.14781F),
new Vector3(-0.070438F, 1.54863F, 0.14898F),
new Vector3(-0.070199F, 1.54897F, 0.14945F),
new Vector3(-0.0700959F, 1.55027F, 0.150494F),
new Vector3(-0.0704095F, 1.55026F, 0.150982F),
new Vector3(-0.070419F, 1.55027F, 0.150997F),
new Vector3(-0.070381F, 1.55035F, 0.151464F),
new Vector3(-0.0710909F, 1.55029F, 0.151401F),
new Vector3(-0.0723465F, 1.54978F, 0.150486F),
new Vector3(-0.0734154F, 1.54921F, 0.149712F),
new Vector3(-0.074418F, 1.54915F, 0.148434F),
new Vector3(-0.0761639F, 1.54857F, 0.146889F),
new Vector3(-0.0784412F, 1.54733F, 0.14536F),
new Vector3(-0.081716F, 1.54556F, 0.142537F),
new Vector3(-0.0828156F, 1.54549F, 0.14077F),
new Vector3(-0.0857415F, 1.54402F, 0.137799F),
new Vector3(-0.0892027F, 1.54149F, 0.134853F),
new Vector3(-0.0922054F, 1.54011F, 0.132308F),
new Vector3(-0.0943632F, 1.54076F, 0.130993F),
new Vector3(-0.097854F, 1.53907F, 0.129302F),
new Vector3(-0.101377F, 1.53651F, 0.127523F),
new Vector3(-0.105134F, 1.53198F, 0.124796F),
new Vector3(-0.108572F, 1.52953F, 0.124192F),
new Vector3(-0.110635F, 1.52927F, 0.124215F),
new Vector3(-0.113341F, 1.5261F, 0.123125F),
new Vector3(-0.116327F, 1.52292F, 0.121938F),
new Vector3(-0.11846F, 1.51957F, 0.119646F),
new Vector3(-0.119758F, 1.51871F, 0.12035F),
new Vector3(-0.121495F, 1.51642F, 0.120835F),
new Vector3(-0.12372F, 1.51367F, 0.119581F),
new Vector3(-0.124546F, 1.5122F, 0.119814F),
new Vector3(-0.126013F, 1.51203F, 0.12006F),
new Vector3(-0.126841F, 1.51007F, 0.119971F),
new Vector3(-0.127753F, 1.50963F, 0.119359F),
new Vector3(-0.128358F, 1.50881F, 0.118802F),
new Vector3(-0.12911F, 1.50897F, 0.118496F),
new Vector3(-0.1292F, 1.5098F, 0.119127F),
new Vector3(-0.129369F, 1.51029F, 0.11881F),
new Vector3(-0.129773F, 1.51088F, 0.118689F),
new Vector3(-0.129579F, 1.51228F, 0.119369F),
new Vector3(-0.129231F, 1.51374F, 0.119214F),
new Vector3(-0.128781F, 1.51614F, 0.119556F),
new Vector3(-0.128597F, 1.51776F, 0.11899F),
new Vector3(-0.128022F, 1.5205F, 0.118625F),
new Vector3(-0.12705F, 1.52332F, 0.118764F),
new Vector3(-0.125922F, 1.5266F, 0.119127F),
new Vector3(-0.124342F, 1.52885F, 0.119741F),
new Vector3(-0.123138F, 1.53096F, 0.121103F),
new Vector3(-0.120998F, 1.53835F, 0.122099F),
new Vector3(-0.121059F, 1.5396F, 0.122294F),
new Vector3(-0.11977F, 1.54114F, 0.122451F),
new Vector3(-0.119048F, 1.54385F, 0.124261F),
new Vector3(-0.117972F, 1.5475F, 0.125456F),
new Vector3(-0.115955F, 1.55125F, 0.126858F),
new Vector3(-0.115134F, 1.55432F, 0.12867F),
new Vector3(-0.114152F, 1.55701F, 0.130651F),
new Vector3(-0.114451F, 1.5586F, 0.131816F),
new Vector3(-0.113462F, 1.56015F, 0.132745F),
new Vector3(-0.112205F, 1.56162F, 0.133294F),
new Vector3(-0.112026F, 1.56257F, 0.133969F),
new Vector3(-0.112798F, 1.56407F, 0.134083F),
new Vector3(-0.112957F, 1.56445F, 0.134081F),
new Vector3(-0.113119F, 1.56391F, 0.1344F),
new Vector3(-0.113845F, 1.56415F, 0.134099F),
new Vector3(-0.115226F, 1.56474F, 0.13316F),
new Vector3(-0.117242F, 1.56367F, 0.131928F),
new Vector3(-0.118274F, 1.5629F, 0.131269F),
new Vector3(-0.12033F, 1.56133F, 0.129978F),
new Vector3(-0.12331F, 1.55961F, 0.128017F),
new Vector3(-0.125546F, 1.55656F, 0.126283F),
new Vector3(-0.126611F, 1.55497F, 0.125963F),
new Vector3(-0.129212F, 1.55204F, 0.125124F),
new Vector3(-0.133406F, 1.54882F, 0.122576F),
new Vector3(-0.13633F, 1.54602F, 0.121548F),
new Vector3(-0.13956F, 1.54342F, 0.120283F),
new Vector3(-0.140718F, 1.54263F, 0.120292F),
new Vector3(-0.143911F, 1.54096F, 0.118867F),
new Vector3(-0.146484F, 1.53807F, 0.116992F),
new Vector3(-0.148511F, 1.53538F, 0.116065F),
new Vector3(-0.15059F, 1.5342F, 0.116317F),
new Vector3(-0.152177F, 1.53192F, 0.115948F),
new Vector3(-0.153323F, 1.52919F, 0.115011F),
new Vector3(-0.154794F, 1.52797F, 0.11497F),
new Vector3(-0.15609F, 1.52761F, 0.115256F),
new Vector3(-0.15734F, 1.52733F, 0.115669F),
new Vector3(-0.157801F, 1.52709F, 0.115723F),
new Vector3(-0.158651F, 1.52627F, 0.115583F),
new Vector3(-0.15876F, 1.52721F, 0.11538F),
new Vector3(-0.158968F, 1.52792F, 0.115295F),
new Vector3(-0.158832F, 1.5293F, 0.115196F),
new Vector3(-0.159547F, 1.53059F, 0.114818F),
new Vector3(-0.159185F, 1.5331F, 0.114501F),
new Vector3(-0.158031F, 1.53792F, 0.114331F),
new Vector3(-0.158026F, 1.53795F, 0.114327F),
new Vector3(-0.158301F, 1.53979F, 0.113918F),
new Vector3(-0.157639F, 1.54198F, 0.113802F),
new Vector3(-0.156858F, 1.54451F, 0.114292F),
new Vector3(-0.156138F, 1.54654F, 0.114855F),
new Vector3(-0.155488F, 1.54951F, 0.115583F),
new Vector3(-0.155859F, 1.55115F, 0.115481F),
new Vector3(-0.154528F, 1.55327F, 0.116289F),
new Vector3(-0.154179F, 1.55574F, 0.117361F),
new Vector3(-0.154071F, 1.55816F, 0.118724F),
new Vector3(-0.154704F, 1.55935F, 0.119637F),
new Vector3(-0.154268F, 1.56099F, 0.120924F),
new Vector3(-0.153759F, 1.5627F, 0.121939F),
new Vector3(-0.154366F, 1.56319F, 0.123128F),
new Vector3(-0.15492F, 1.56397F, 0.123582F),
new Vector3(-0.15563F, 1.56339F, 0.124099F),
new Vector3(-0.156954F, 1.56315F, 0.124841F),
new Vector3(-0.158333F, 1.56226F, 0.12463F),
new Vector3(-0.159126F, 1.56044F, 0.123898F),
new Vector3(-0.16117F, 1.55922F, 0.123593F),
new Vector3(-0.162436F, 1.55797F, 0.123022F),
new Vector3(-0.164237F, 1.5552F, 0.122009F),
new Vector3(-0.166283F, 1.5525F, 0.12059F),
new Vector3(-0.168572F, 1.54938F, 0.119223F),
new Vector3(-0.170783F, 1.54579F, 0.117039F),
new Vector3(-0.171654F, 1.54397F, 0.116018F),
new Vector3(-0.173166F, 1.54095F, 0.114954F),
new Vector3(-0.175184F, 1.53717F, 0.113248F),
new Vector3(-0.175655F, 1.53538F, 0.112821F),
new Vector3(-0.176968F, 1.53308F, 0.112616F),
new Vector3(-0.177128F, 1.53353F, 0.113109F),
new Vector3(-0.179048F, 1.5316F, 0.112456F),
new Vector3(-0.180086F, 1.5318F, 0.113011F),
new Vector3(-0.181295F, 1.53224F, 0.112403F),
new Vector3(-0.181849F, 1.53285F, 0.111973F),
new Vector3(-0.18287F, 1.5343F, 0.112235F),
new Vector3(-0.183462F, 1.53601F, 0.112358F),
new Vector3(-0.184465F, 1.53749F, 0.111971F),
new Vector3(-0.184173F, 1.54018F, 0.112339F),
new Vector3(-0.184775F, 1.54308F, 0.111508F),
new Vector3(-0.185955F, 1.54481F, 0.110501F),
new Vector3(-0.186498F, 1.54726F, 0.110659F),
new Vector3(-0.185983F, 1.54841F, 0.111376F),
new Vector3(-0.187436F, 1.55268F, 0.113452F),
new Vector3(-0.187471F, 1.55283F, 0.113476F),
new Vector3(-0.189397F, 1.55443F, 0.113345F),
new Vector3(-0.189741F, 1.55725F, 0.11451F),
new Vector3(-0.19102F, 1.56023F, 0.115502F),
new Vector3(-0.192361F, 1.56503F, 0.117088F),
new Vector3(-0.195367F, 1.57111F, 0.117661F)
    #endregion
  };
  private float[] _previewStrokeDeltaTimes = new float[] {
    #region Preview Stroke Delta Time Data
0.01284074F,
0.01355068F,
0.01367515F,
0.01731882F,
0.009912463F,
0.01239387F,
0.01339092F,
0.01347561F,
0.01317662F,
0.01354394F,
0.01303707F,
0.01352598F,
0.01375086F,
0.02670677F,
0.005005154F,
0.007790357F,
0.01321608F,
0.0133999F,
0.01311343F,
0.01358533F,
0.01316475F,
0.01366039F,
0.01351154F,
0.01314615F,
0.01357795F,
0.01309642F,
0.01348844F,
0.01673111F,
0.009852473F,
0.01359655F,
0.01286352F,
0.01350897F,
0.01417015F,
0.01254208F,
0.01362254F,
0.01291581F,
0.0136729F,
0.01344546F,
0.01338643F,
0.01684403F,
0.009502798F,
0.01342974F,
0.01349775F,
0.0130807F,
0.01365654F,
0.01293859F,
0.01365334F,
0.01377556F,
0.01280128F,
0.0136867F,
0.01301301F,
0.01677506F,
0.009708432F,
0.0136851F,
0.01324143F,
0.01305921F,
0.01370659F,
0.01361388F,
0.02665384F,
0.001949514F,
0.01096566F,
0.01312113F,
0.01318336F,
0.01666855F,
0.0106317F,
0.01294019F,
0.01346695F,
0.01347016F,
0.01303996F,
0.0135109F,
0.01309739F,
0.01368766F,
0.01288726F,
0.01363762F,
0.01365654F,
0.01791262F,
0.008472062F,
0.01315866F,
0.01318785F,
0.01364916F,
0.01312626F,
0.01343615F,
0.0129588F,
0.01364467F,
0.01367547F,
0.01298992F,
0.01408129F,
0.01571289F,
0.01056947F,
0.01354651F,
0.01299922F,
0.01336814F,
0.01311856F,
0.01351026F,
0.01357891F,
0.01319138F,
0.01344546F,
0.01302777F,
0.01378903F,
0.01665701F,
0.009777405F,
0.01370563F,
0.02666667F,
0.002106065F,
0.01065704F,
0.01304157F,
0.01347337F,
0.01313524F,
0.01353688F,
0.01306434F,
0.01374476F,
0.01676383F,
0.009772593F,
0.01373514F,
0.01292287F,
0.01352437F,
0.01378968F,
0.01277723F,
0.01362093F,
0.01285839F,
0.01370755F,
0.01347016F,
0.01345508F,
0.01678276F,
0.009458528F,
0.01370274F,
0.01330334F,
0.01305825F,
0.01362575F,
0.0130082F,
0.01345412F,
0.01369793F,
0.01302585F,
0.01347016F,
0.01334537F,
0.01633588F,
0.01049472F,
0.01318464F,
0.01334023F,
0.01295591F,
0.01379385F,
0.01277145F,
0.01364884F,
0.01362286F,
0.01319748F,
0.01326356F,
0.02689765F,
0.006778548F,
0.006495279F,
0.01488361F,
0.0132395F,
0.01342589F,
0.01265019F,
0.01356479F,
0.01258956F,
0.01361709F,
0.01285358F,
0.0134554F,
0.02654862F,
0.005497585F,
0.007686738F,
0.01259597F,
0.01344802F,
0.01350256F,
0.01306498F,
0.01351635F,
0.0136944F,
0.01317406F,
0.01350416F,
0.01290972F,
0.01379256F,
0.01296522F,
0.01671475F,
0.009733134F,
0.0136466F,
0.01330911F,
0.01306306F,
0.01371653F,
0.01351699F,
0.01303996F,
0.01346342F,
0.01324463F,
0.01354426F,
0.01358083F,
0.0161912F,
0.01115076F,
0.01208333F,
0.01344096F,
0.01365173F,
0.01296265F,
0.02683541F,
0.002150015F,
0.01200762F,
0.01186037F,
0.01358565F,
0.01358757F,
0.01635962F,
0.01054508F,
0.01294629F,
0.01321672F,
0.01369215F,
0.01301301F,
0.01352854F,
0.01312979F,
0.01352052F,
0.01436167F,
0.01241825F,
0.0136158F,
0.01601733F,
0.01066731F,
0.01331681F,
0.01295463F,
0.01363633F,
0.01292383F,
0.01350609F,
0.01370146F,
0.01291517F,
0.01369151F,
0.01365783F,
0.01297131F,
0.01709907F,
0.009444091F,
0.01359431F,
0.0128138F,
0.01379994F,
0.01277017F,
0.01356223F,
0.01370402F,
0.01297131F,
0.01348588F,
0.01370081F,
0.01311663F,
0.01663166F,
0.01005394F,
0.02678088F,
0.002344101F,
0.01075457F,
0.01303355F,
0.01289945F,
0.01366905F,
0.0130868F

    #endregion
  };
  private Vector3 _previewStrokeLocalCenter = -1F
    * (new Vector3(-0.226124F, 1.48719F, 0.0955172F)
    + new Vector3(-0.0978155F, 1.41659F, 0.0789379F)
    + new Vector3(-0.114451F, 1.5586F, 0.131816F)
    + new Vector3(-0.195367F, 1.57111F, 0.117661F)) / 4F;
  private float _previewStrokeScalingFactor = 0.9F;  // scales the preview stroke Vector3s, not the tube itself

  private List<TubeStroke> _tubeStrokes = new List<TubeStroke>();
  private List<TubeStroke> _undoneTubeStrokes = new List<TubeStroke>();

  #endregion

  #region PROPERTIES

  public bool IsCurrentlyDrawing {
    get;
    private set;
  }

  #endregion

  #region UNITY EVENTS

  public StringEvent OnSaveSuccessful;

  #endregion

  #region UNITY CALLBACKS

  void OnValidate() {
    _tubeRadius = Mathf.Max(0, _tubeRadius);
    _tubeResolution = Mathf.Clamp(_tubeResolution, 3, 24);
    _tubeMinSegmentLength = Mathf.Max(0, _tubeMinSegmentLength);
  }

  void Awake() {
    if (_pinchDetectors.Length == 0) {
      Debug.LogWarning("No pinch detectors were specified!  PinchDrawing can not draw any lines without PinchDetectors.");
    }
  }

  void Start() {
    // Primary DrawState and undo/redo history
    _drawState = new DrawState(this);
    _undoHistory = new List<GameObject>();
    _redoHistory = new List<GameObject>();

    // Preview DrawState
    _previewDrawState = new DrawState(this);
    RefreshPreviewStroke();
  }

  void Update() {
    for (int i = 0; i < _pinchDetectors.Length; i++) {
      var detector = _pinchDetectors[i];

      if (detector == null) return;

      if (detector.DidStartPinch && (_dontDrawDetector != null && !_dontDrawDetector.IsActive)) {
        IsCurrentlyDrawing = true;

        _undoHistory.Add(_drawState.BeginNewLine());
        ClearRedoHistory();
      }
      if (detector.DidEndPinch && IsCurrentlyDrawing) {
        _drawState.FinishLine();
        IsCurrentlyDrawing = false;
      }
      if (detector.IsPinching && IsCurrentlyDrawing) {
        _drawState.UpdateLine(detector.Position);
      }
    }
  }

  #endregion

  #region PRIVATE METHODS

  private void ClearRedoHistory() {
    for (int i = 0; i < _redoHistory.Count; i++) {
      Destroy(_redoHistory[i]);
    }
    _redoHistory.Clear();

    // TubeStroke undo tracking
    _undoneTubeStrokes.Clear();
  }

  /// <summary>
  /// Deletes the old preview tube and draws a new one
  /// using the preview DrawState and the current tube settings.
  /// </summary>
  private void RefreshPreviewStroke() {
    DrawState previewDrawState = _previewDrawState;

    if (_previewTube != null) {
      Destroy(_previewTube);
    }
    _previewTube = previewDrawState.BeginNewLine();
    _previewTube.transform.parent = _previewStrokeTransform;
    _previewTube.transform.localPosition = _previewStrokeLocalCenter * _previewStrokeScalingFactor;
    _previewTube.transform.localRotation = Quaternion.identity;

    float timeSkipped = 0F;
    for (int i = 0; i < _previewStrokePoints.Length; i++) {
      if (i % 3 != 0) {
        timeSkipped += _previewStrokeDeltaTimes[i];
      }
      else {
        previewDrawState.UpdateLine(_previewStrokePoints[i] * _previewStrokeScalingFactor, _previewStrokeDeltaTimes[i] + timeSkipped);
        timeSkipped = 0F;
      }
    }

    if (_displayPreviewStroke) {
      _previewTube.GetComponentInChildren<Renderer>().enabled = true;
    }
    else {
      _previewTube.GetComponentInChildren<Renderer>().enabled = false;
    }

    //for (int i = 0; i < _previewStrokePoints.Length - 1; i++) {
    //  Vector2 strokeStart = _previewStrokePoints[i];
    //  Vector2 strokeEnd   = _previewStrokePoints[i + 1];

    //  for (int j = 0; j < _previewStrokePointsPerSegment; j++) {
    //    Vector2 curPoint = Vector2.Lerp(strokeStart, strokeEnd, (float)j / _previewStrokePointsPerSegment);
    //    previewDrawState.UpdateLine(curPoint * _previewStrokeScalingFactor, _previewStrokeTimePerPoint);
    //  }
    //}

    //// To account for the smoothing delay, update with the last stroke point a few more times before finishing the line
    //for (int i = 0; i < _previewStrokePointsPerSegment * 5; i++) {
    //  previewDrawState.UpdateLine(_previewStrokePoints[_previewStrokePoints.Length - 1] * _previewStrokeScalingFactor, _previewStrokeTimePerPoint);
    //}

    previewDrawState.FinishLine();
  }

  #endregion

  #region PUBLIC METHODS

  /// <summary>
  /// Hides the last-draw tube object created and adds it to the redo history.
  /// More presses will hide older tubes.
  /// </summary>
  public void Undo() {
    if (_undoHistory.Count > 0) {
      GameObject toUndo = _undoHistory[_undoHistory.Count - 1];
      _undoHistory.RemoveAt(_undoHistory.Count - 1);
      toUndo.SetActive(false);
      _redoHistory.Add(toUndo);

      // TubeStroke undo tracking
      _undoneTubeStrokes.Add(_tubeStrokes[_undoHistory.Count - 1]);
      _tubeStrokes.RemoveAt(_undoHistory.Count - 1);
    }
  }

  /// <summary>
  /// Unhides the last-undone tube object and re-adds it to the undo history.
  /// More presses will un-undo more recent tubes. Upon making a new stroke,
  /// the redo history is cleared and its objects are deleted.
  /// </summary>
  public void Redo() {
    if (_redoHistory.Count > 0) {
      GameObject toRedo = _redoHistory[_redoHistory.Count - 1];
      _redoHistory.RemoveAt(_redoHistory.Count - 1);
      toRedo.SetActive(true);
      _undoHistory.Add(toRedo);

      // TubeStroke redo tracking
      _tubeStrokes.Add(_undoneTubeStrokes[_redoHistory.Count - 1]);
      _undoneTubeStrokes.RemoveAt(_redoHistory.Count - 1);
    }
  }

  /// <summary>
  /// Sets drawn tube color.
  /// </summary>
  public void SetColor(Color color) {
    _tubeColor = color;
    RefreshPreviewStroke();
  }

  /// <summary>
  /// Sets drawn tube thickness. Expects a value from 0 (min) to 1 (max).
  /// </summary>
  public void SetThickness(float normalizedThickness) {
    _tubeRadius = Mathf.Lerp(0.001F, 0.01F, normalizedThickness);
    RefreshPreviewStroke();
  }

  /// <summary>
  /// Sets the tube smoothing delay. Expects a value from 0 (min) to 1 (max);
  /// </summary>
  public void SetSmoothing(float normalizedSmoothing) {
    _tubeSmoothingDelay = Mathf.Lerp(0F, 0.15F, normalizedSmoothing);
    RefreshPreviewStroke();
  }

  public void DisplayPreviewStroke() {
    _displayPreviewStroke = true;
    RefreshPreviewStroke();
  }

  public void HidePreviewStroke() {
    _displayPreviewStroke = false;
    RefreshPreviewStroke();
  }

  public void Save(string filePath) {
    SavedScene toSave = new SavedScene();
    toSave._tubeStrokes = new TubeStroke[_tubeStrokes.Count];
    for (int i = 0; i < _tubeStrokes.Count; i++) {
      toSave._tubeStrokes[i] = _tubeStrokes[i];
    }

    string savedSceneJSON = toSave.WriteToJSON();

    if (File.Exists(filePath)) {
      File.Delete(filePath);
    }
    using (StreamWriter writer = File.CreateText(filePath)) {
      writer.Write(savedSceneJSON);
    }

    OnSaveSuccessful.Invoke("Saved " + Path.GetFileName(filePath));
  }

  public void Load(string filePath) {
    // Clear all strokes -- TODO: make this not just use undo
    for (int i = 0; i < _undoHistory.Count; i++) {
      Undo();
    }
    ClearRedoHistory();

    // Get JSON string from filename
    StreamReader reader = new StreamReader(filePath);
    Debug.Log("Trying to read path: " + filePath);
    string json = reader.ReadToEnd();

    Debug.Log("Got json: " + json);

    // Load SavedScene object from JSON
    SavedScene savedScene = SavedScene.CreateFromJSON(json);

    // Recreate each tube from stroke data
    for (int i = 0; i < savedScene._tubeStrokes.Length; i++) {
      TubeStroke tubeStroke = savedScene._tubeStrokes[i];

      _tubeRadius = tubeStroke._radius;
      _tubeColor = tubeStroke._color;
      _tubeResolution = tubeStroke._resolution;
      _tubeSmoothingDelay = tubeStroke._smoothingDelay;
      _drawState.BeginNewLine();
      for (int j = 0; j < tubeStroke._strokePoints.Count; j++) {
        _drawState.UpdateLine(tubeStroke._strokePoints[j], tubeStroke._strokePointDeltaTimes[j]);
      }
      _drawState.FinishLine();
    }
  }

  #endregion

  #region DEBUG OUTPUT (TODO DELETEME)

  // TODO: REMOVE SYSTEM.IO DEPENDENCY
  // TODO: REMOVE TEXT DEPENDENCY

  // TODO: DELETEME
  // Recording a line for the preview display
  public List<Vector3> points = new List<Vector3>();
  public List<float> deltaTimes = new List<float>();
  public void RecordPoint(Vector3 point) {
    points.Add(point);
  }
  public void RecordDeltaTime(float deltaTime) {
    deltaTimes.Add(deltaTime);
  }
  public void OutputPoints() {
    return; // comment out to get output.txt
    
    // TODO: DELETEME DEBUG STROKE OUTPUT
    if (File.Exists("./output.txt")) {
      File.Delete("./output.txt");
    }
    StreamWriter writer = new StreamWriter("./output.txt");
    StringBuilder line = new StringBuilder();
    for (int i = 0; i < points.Count; i++) {
      Vector3 vec = points[i];
      line.Append("new Vector3(");
      line.Append(vec.x.ToString("G6") + "F, ");
      line.Append(vec.y.ToString("G6") + "F, ");
      line.Append(vec.z.ToString("G6") + "F)");
      if (i != points.Count - 1) line.Append(",");
      line.Append("\n");
    }
    line.Append("\n");
    for (int i = 0; i < points.Count; i++) {
      float dt = deltaTimes[i];
      line.Append(dt + "F");
      if (i != points.Count - 1) line.Append(",");
      line.Append("\n");
    }
    writer.Write(line.ToString());
    writer.Close();

    points.Clear();
    deltaTimes.Clear();
  }

  #endregion

  #region DRAWSTATE

  // TODO: Look into Alex's more generalized Mesh drawing code

  private class DrawState {
    private List<Vector3> _vertices = new List<Vector3>();
    private List<int> _tris = new List<int>();
    private List<Vector2> _uvs = new List<Vector2>();
    private List<Color> _colors = new List<Color>();

    private PinchDrawing _parent;

    private int _rings = 0;

    private Vector3 _prevRing0 = Vector3.zero;
    private Vector3 _prevRing1 = Vector3.zero;

    private Vector3 _prevNormal0 = Vector3.zero;

    private Mesh _mesh;
    private SmoothedVector3 _smoothedPosition;

    // quick-and-dirty I/O support
    private TubeStroke _curTubeStroke = null;

    public DrawState(PinchDrawing parent) {
      _parent = parent;

      _smoothedPosition = new SmoothedVector3();
      _smoothedPosition.delay = parent._tubeSmoothingDelay;
      _smoothedPosition.reset = true;
    }

    public GameObject BeginNewLine() {

      _rings = 0;
      _vertices.Clear();
      _tris.Clear();
      _uvs.Clear();
      _colors.Clear();

      _smoothedPosition.reset = true;
      _smoothedPosition.delay = _parent._tubeSmoothingDelay;

      _mesh = new Mesh();
      _mesh.name = "Line Mesh";
      _mesh.MarkDynamic();

      GameObject lineObj = new GameObject("Line Object");
      lineObj.transform.position = Vector3.zero;
      lineObj.transform.rotation = Quaternion.identity;
      lineObj.transform.localScale = Vector3.one;
      lineObj.AddComponent<MeshFilter>().mesh = _mesh;
      lineObj.AddComponent<MeshRenderer>().sharedMaterial = _parent._tubeMaterial;

      // quick-and-dirty I/O support
      if (this == _parent._drawState) { // ignore _previewDrawState
        _curTubeStroke = new TubeStroke();
        _curTubeStroke._radius = _parent._tubeRadius;
        _curTubeStroke._color = _parent._tubeColor;
        _curTubeStroke._resolution = _parent._tubeResolution;
        _curTubeStroke._smoothingDelay = _parent._tubeSmoothingDelay;
      }

      return lineObj;
    }

    /// <summary>
    /// Updates the line currently being drawn with a new position and an argument
    /// deltaTime between the new position and the last.
    /// Manually specifying a time is probably only necessary if you're programmatically
    /// reconstructing a stroke; normal interactions will call UpdateLine(Vector3), which
    /// will just use Time.deltaTime.
    /// </summary>
    public void UpdateLine(Vector3 position, float deltaTime) {

      // TODO: DELETEME
      // Recording a line for the preview display
      _parent.RecordPoint(position);
      _parent.RecordDeltaTime(deltaTime);

      // quick-and-dirty I/O support
      if (this == _parent._drawState) { // ignore _previewDrawState
        _curTubeStroke.RecordStrokePoint(position, deltaTime);
      }

      _smoothedPosition.Update(position, deltaTime);

      bool shouldAdd = false;

      shouldAdd |= _vertices.Count == 0;
      shouldAdd |= Vector3.Distance(_prevRing0, _smoothedPosition.value) >= _parent._tubeMinSegmentLength;
      shouldAdd |= (this == _parent._previewDrawState && Vector3.Distance(_prevRing0, _smoothedPosition.value) >= _parent._tubeMinSegmentLength * _parent._previewStrokeScalingFactor);

      if (shouldAdd) {
        addRing(_smoothedPosition.value);
        updateMesh();
      }
    }
    public void UpdateLine(Vector3 position) {
      UpdateLine(position, Time.deltaTime);
    }

    public void FinishLine() {
      // TODO: DELETEME (outputting last drawn line)
      _parent.OutputPoints();

      // quick-and-dirty I/O support
      if (this == _parent._drawState) { // ignore preview drawstate things
        _parent._tubeStrokes.Add(_curTubeStroke);
      }

      _mesh.Optimize();
      _mesh.UploadMeshData(true);
    }

    private void updateMesh() {
      _mesh.SetVertices(_vertices);
      _mesh.SetColors(_colors);
      _mesh.SetUVs(0, _uvs);
      _mesh.SetIndices(_tris.ToArray(), MeshTopology.Triangles, 0);
      _mesh.RecalculateBounds();
      _mesh.RecalculateNormals();
    }

    private void addRing(Vector3 ringPosition) {
      _rings++;

      if (_rings == 1) {
        addVertexRing();
        addVertexRing();
        addTriSegment();
      }

      addVertexRing();
      addTriSegment();

      Vector3 ringNormal = Vector3.zero;
      if (_rings == 2) {
        Vector3 direction = ringPosition - _prevRing0;
        float angleToUp = Vector3.Angle(direction, Vector3.up);

        if (angleToUp < 10 || angleToUp > 170) {
          ringNormal = Vector3.Cross(direction, Vector3.right);
        }
        else {
          ringNormal = Vector3.Cross(direction, Vector3.up);
        }

        ringNormal = ringNormal.normalized;

        _prevNormal0 = ringNormal;
      }
      else if (_rings > 2) {
        Vector3 prevPerp = Vector3.Cross(_prevRing0 - _prevRing1, _prevNormal0);
        ringNormal = Vector3.Cross(prevPerp, ringPosition - _prevRing0).normalized;
      }

      if (_rings == 2) {
        updateRingVerts(0,
                        _prevRing0,
                        ringPosition - _prevRing1,
                        _prevNormal0,
                        0);
      }

      if (_rings >= 2) {
        updateRingVerts(_vertices.Count - _parent._tubeResolution,
                        ringPosition,
                        ringPosition - _prevRing0,
                        ringNormal,
                        0);
        updateRingVerts(_vertices.Count - _parent._tubeResolution * 2,
                        ringPosition,
                        ringPosition - _prevRing0,
                        ringNormal,
                        1);
        updateRingVerts(_vertices.Count - _parent._tubeResolution * 3,
                        _prevRing0,
                        ringPosition - _prevRing1,
                        _prevNormal0,
                        1);
      }

      _prevRing1 = _prevRing0;
      _prevRing0 = ringPosition;

      _prevNormal0 = ringNormal;
    }

    private void addVertexRing() {
      for (int i = 0; i < _parent._tubeResolution; i++) {
        _vertices.Add(Vector3.zero);  //Dummy vertex, is updated later
        _uvs.Add(new Vector2(i / (_parent._tubeResolution - 1.0f), 0));
        _colors.Add(_parent._tubeColor);
      }
    }

    //Connects the most recently added vertex ring to the one before it
    private void addTriSegment() {
      for (int i = 0; i < _parent._tubeResolution; i++) {
        int i0 = _vertices.Count - 1 - i;
        int i1 = _vertices.Count - 1 - ((i + 1) % _parent._tubeResolution);

        _tris.Add(i0);
        _tris.Add(i1 - _parent._tubeResolution);
        _tris.Add(i0 - _parent._tubeResolution);

        _tris.Add(i0);
        _tris.Add(i1);
        _tris.Add(i1 - _parent._tubeResolution);
      }
    }

    private void updateRingVerts(int offset, Vector3 ringPosition, Vector3 direction, Vector3 normal, float radiusScale) {
      direction = direction.normalized;
      normal = normal.normalized;

      for (int i = 0; i < _parent._tubeResolution; i++) {
        float angle = 360.0f * (i / (float)(_parent._tubeResolution));
        Quaternion rotator = Quaternion.AngleAxis(angle, direction);
        Vector3 ringSpoke = rotator * normal * _parent._tubeRadius * radiusScale;
        _vertices[offset + i] = ringPosition + ringSpoke;
      }
    }
  }

  #endregion

}