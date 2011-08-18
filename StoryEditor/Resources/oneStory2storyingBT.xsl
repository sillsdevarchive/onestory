<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE stylesheet [
<!ENTITY cr "&#xD;&#xA;">
<!ENTITY tab "&#9;">
<!ENTITY nbsp "&#160;">
]>
<xsl:transform version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
			   xmlns:msxsl="urn:schemas-microsoft-com:xslt"
			   xmlns:user="urn:my-scripts">
  <msxsl:script language="C#" implements-prefix="user">
	<msxsl:assembly name="System.Windows.Forms" />
	<![CDATA[
  public string Rtf2Text(string strRtf)
  {
	System.Windows.Forms.RichTextBox rtBox = new System.Windows.Forms.RichTextBox();
	rtBox.Rtf = strRtf;
	return rtBox.Text;
  }
  public class BookMap : System.Collections.Generic.Dictionary<string,string>
  {
	public BookMap()
	{
	  Add("Gen", "Gen");
	  Add("Exod", "Exo");
	  Add("Lev", "Lev");
	  Add("Num", "Num");
	  Add("Deut", "Deu");
	  Add("Josh", "Jos");
	  Add("Judg", "Jdg");
	  Add("Ruth", "Rut");
	  Add("1Sam", "1Sa");
	  Add("2Sam", "2Sa");
	  Add("1Kgs", "1Ki");
	  Add("2Kgs", "2Ki");
	  Add("1Chr", "1Ch");
	  Add("2Chr", "2Ch");
	  Add("Ezra", "Ezr");
	  Add("Neh", "Neh");
	  Add("Esth", "Est");
	  Add("Job", "Job");
	  Add("Ps", "Psa");
	  Add("Prov", "Pro");
	  Add("Eccl", "Ecc");
	  Add("Song", "Sng");
	  Add("Isa", "Isa");
	  Add("Jer", "Jer");
	  Add("Lam", "Lam");
	  Add("Ezek", "Ezk");
	  Add("Dan", "Dan");
	  Add("Hos", "Hos");
	  Add("Joel", "Jol");
	  Add("Amos", "Amo");
	  Add("Obad", "Oba");
	  Add("Jonah", "Jon");
	  Add("Mic", "Mic");
	  Add("Nah", "Nam");
	  Add("Hab", "Hab");
	  Add("Zeph", "Zep");
	  Add("Hag", "Hag");
	  Add("Zech", "Zec");
	  Add("Mal", "Mal");
	  Add("Matt", "Mat");
	  Add("Mark", "Mrk");
	  Add("Luke", "Luk");
	  Add("John", "Jhn");
	  Add("Acts", "Act");
	  Add("Rom", "Rom");
	  Add("1Cor", "1Co");
	  Add("2Cor", "2Co");
	  Add("Gal", "Gal");
	  Add("Eph", "Eph");
	  Add("Phil", "Php");
	  Add("Col", "Col");
	  Add("1Thess", "1Th");
	  Add("2Thess", "2Th");
	  Add("1Tim", "1Ti");
	  Add("2Tim", "2Ti");
	  Add("Titus", "Tit");
	  Add("Phlm", "Phm");
	  Add("Heb", "Heb");
	  Add("Jas", "Jas");
	  Add("1Pet", "1Pe");
	  Add("2Pet", "2Pe");
	  Add("1John", "1Jn");
	  Add("2John", "2Jn");
	  Add("3John", "3Jn");
	  Add("Jude", "Jud");
	  Add("Rev", "Rev");
	}
  }
  BookMap map = new BookMap();
  public string ConvertToToolboxAnchor(string strAnchor)
  {
	if (strAnchor == "No Anchor")
	  return strAnchor; // the following code doesn't apply if this is a null anchor
	int nIndexSpace = strAnchor.IndexOf(' ');
	string strBookCode = strAnchor.Substring(0, nIndexSpace);
	try
	{
	  strBookCode = map[strBookCode];
	}
	catch
	{
	  // ignore
	}
	string[] astrChVrs = strAnchor.Substring(nIndexSpace+1).Split(new char[] { ':' });
	int nLen = 2;
	string strChap = astrChVrs[0];
	string strVerse = astrChVrs[1];
	while (strVerse.Length < nLen)
	  strVerse = "0" + strVerse;
	if (strBookCode == "Psa")
	  nLen++;
	while (strChap.Length < nLen)
	  strChap = "0" + strChap;
	return strBookCode + "." + strChap + ":" + strVerse;
  }
  ]]>
  </msxsl:script>

  <xsl:variable name="storySet">
	<xsl:text>Stories</xsl:text>
  </xsl:variable>
  <xsl:variable name="pathToRunningFolder">
	<xsl:text>C:\src\StoryEditor\output\Release\StageTransitions.xml</xsl:text>
  </xsl:variable>
  <!--
  <xsl:param name="storySet"/>
  <xsl:variable name="pathToRunningFolder">
	<xsl:text>{0}\Release\StageTransitions.xml</xsl:text>
  </xsl:variable>
  -->

  <xsl:output method="text" encoding="UTF-8" indent="no"/>
	<xsl:strip-space elements="*"/>

	<!-- oneStory2storyingBT.xsl
		create by Jim Albright to handle conversion of onestory to sfm
		2010-04-03
	-->

	<xsl:template match="StoryProject">
	  <xsl:text>\_sh v3.0  374  StoryBT</xsl:text>
	  <xsl:text>&cr;&cr;\c 00</xsl:text>
	  <xsl:text>&cr;\t </xsl:text>
	  <xsl:value-of select="@ProjectName"/>
	  <xsl:text>&cr;\co Language: </xsl:text>
	  <xsl:value-of select="preceding::Languages/LanguageInfo[@lang='Vernacular']/@name"/>
	  <xsl:variable name="RtfFrontMatter" select="@PanoramaFrontMatter"></xsl:variable>
	  <xsl:text>&cr;&cr;\co Front Matter:&cr;</xsl:text>
	  <xsl:value-of select="user:Rtf2Text($RtfFrontMatter)"/>
	  <xsl:text>&cr;&cr;\co Team Members:</xsl:text>
	  <xsl:apply-templates select="Members"/>
	  <xsl:apply-templates select="stories"/>
	</xsl:template>

  <xsl:template match="Members">
	<xsl:apply-templates select="Member"/>
  </xsl:template>

  <xsl:template match="Member[not(@name='Browser')]">
	<xsl:text>&cr;\co  </xsl:text>
	<xsl:value-of select="@name"/>
	<xsl:text> (</xsl:text>
	<xsl:value-of select="@memberType"/>
	<xsl:text>): </xsl:text>
	<xsl:value-of select="@bioData"/>
  </xsl:template>

  <xsl:template match="stories">
	<xsl:if test="@SetName = $storySet">
	  <xsl:apply-templates select="story"/>
	</xsl:if>
  </xsl:template>

<xsl:template match="story">
  <xsl:text>&cr;&cr;\c </xsl:text>
  <xsl:number value="position()" format="01" />
  <xsl:text>&cr;\t </xsl:text><xsl:value-of select="@name"/>
  <xsl:text>&cr;\co </xsl:text><xsl:value-of select="preceding::Languages/LanguageInfo[@lang='Vernacular']/@name"/>
  <xsl:variable name="projectFacilitator" select="descendant::ProjectFacilitator/@memberID"/>
  <xsl:text>&cr;&cr;\co Project Facilitator: </xsl:text>
  <xsl:value-of select="preceding::Member[@memberKey=$projectFacilitator]/@name"/>
  <xsl:variable name="storyCrafter" select="descendant::StoryCrafter/@memberID"/>
  <xsl:text>&cr;\co Story crafter: </xsl:text><xsl:value-of select="preceding::Member[@memberKey=$storyCrafter]/@name"/>
  <xsl:text>&cr;\co Resources used: </xsl:text>
  <xsl:value-of select="descendant::ResourcesUsed"/>
  <xsl:variable name="BackTranslator" select="descendant::BackTranslator/@memberID"/>
  <xsl:text>&cr;\co Backtranslator: </xsl:text><xsl:value-of select="preceding::Member[@memberKey=$BackTranslator]/@name"/>
  <xsl:text>&cr;\co reason this story is in the set: </xsl:text><xsl:value-of select="descendant::StoryPurpose"/>
  <xsl:variable name="stageId" select="concat('e', @stage)"/>
  <xsl:text>&cr;\co Story State: </xsl:text>
  <xsl:value-of select="document($pathToRunningFolder)/ProjectStates/StateTransition[@stage = $stageId]/StageDisplayString"/>
  <xsl:apply-templates select="descendant::TestRetelling"/>
  <xsl:apply-templates select="descendant::TestTqAnswer"/>
  <xsl:text>&cr;</xsl:text>
  <xsl:apply-templates select="descendant::Verses">
	<xsl:with-param name="chapNo" select="position()" />
  </xsl:apply-templates>
</xsl:template>

<xsl:template match="TestRetelling">
	<xsl:text>&cr;\co Retelling Testing </xsl:text>
	<xsl:value-of select="count(preceding-sibling::TestRetelling)+1"></xsl:value-of>
	<xsl:variable name="Testing" select="@memberID"/>
	<xsl:text>: </xsl:text>
	<xsl:value-of select="preceding::Member[@memberKey=$Testing]/@name"/>
</xsl:template>

  <xsl:template match="TestTqAnswer">
	<xsl:text>&cr;\co Inference Testing </xsl:text>
	<xsl:value-of select="count(preceding-sibling::TestTqAnswer)+1"></xsl:value-of>
	<xsl:variable name="Testing" select="@memberID"/>
	<xsl:text>: </xsl:text>
	<xsl:value-of select="preceding::Member[@memberKey=$Testing]/@name"/>
  </xsl:template>

  <xsl:template match="Verses">
	  <xsl:param name="chapNo" />
	  <xsl:apply-templates>
		<xsl:with-param name="chapNo" select="$chapNo" />
	  </xsl:apply-templates>
	</xsl:template>
	<xsl:template match="Verse">
	  <xsl:param name="chapNo" />
	  <xsl:text>&cr;\ln </xsl:text><xsl:number value="$chapNo" format="01" />
	  <xsl:text>.</xsl:text>
	  <xsl:number value="position()-1" format="01" />
	  <xsl:if test="@visible = 'false'">
		<xsl:text> (hidden)</xsl:text>
	  </xsl:if>
	  <xsl:if test="position() = 1">
		<xsl:text>0 (general testing questions)</xsl:text>
	  </xsl:if>
	  <xsl:if test="StoryLine[@lang='Vernacular'][text()] and preceding::Languages/LanguageInfo[@lang='Vernacular'][@code]">
		<xsl:text>&cr;\</xsl:text>
		<xsl:value-of select="preceding::Languages/LanguageInfo[@lang='Vernacular']/@code"/>
		<xsl:text> </xsl:text>
		<xsl:value-of select="descendant::StoryLine[@lang='Vernacular']"/>
	  </xsl:if>
	  <xsl:if test="StoryLine[@lang='NationalBt'][text()] and preceding::Languages/LanguageInfo[@lang='NationalBt'][@code]">
		<xsl:text>&cr;\bt</xsl:text>
		<xsl:value-of select="preceding::Languages/LanguageInfo[@lang='NationalBt']/@code"/>
		<xsl:text> </xsl:text>
		<xsl:value-of select="descendant::StoryLine[@lang='NationalBt']"/>
	  </xsl:if>
	  <xsl:if test="StoryLine[@lang='InternationalBt'][text()] and preceding::Languages/LanguageInfo[@lang='InternationalBt'][@code]">
		<xsl:text>&cr;\bt</xsl:text>
		<xsl:value-of select="preceding::Languages/LanguageInfo[@lang='InternationalBt']/@code"/>
		<xsl:text> </xsl:text>
		<xsl:value-of select="descendant::StoryLine[@lang='InternationalBt']"/>
	  </xsl:if>
	  <xsl:apply-templates select="Anchors"/>
	  <xsl:apply-templates select="ExegeticalHelps"/>
	  <xsl:apply-templates select="TestQuestions" />
	</xsl:template>

  <xsl:template match="Anchors">
	<xsl:text>&cr;\anc </xsl:text>
	<xsl:for-each select="Anchor">
	  <xsl:variable name="ancJT" select="@jumpTarget"/>
	  <xsl:value-of select="user:ConvertToToolboxAnchor($ancJT)"/>
	  <xsl:if test="position()>0 and position()!=last()">
		<xsl:text>; </xsl:text>
	  </xsl:if>
	</xsl:for-each>
  </xsl:template>

  <xsl:template match="ExegeticalHelps">
	<xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="ExegeticalHelp">
	<xsl:text>&cr;\cn </xsl:text>
	<xsl:value-of select="node()"/>
  </xsl:template>

  <xsl:template match="TestQuestions">
	  <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="TestQuestion">
	<!--I'm hiding this, because it turns out I don't want anything different
			however, this is a nice equation:
			get the position of the ancestor Verse (in this case, it means the FirstVerse
	<xsl:variable name="tstType">
	  <xsl:choose>
		<xsl:when test="count(ancestor::Verse/*/preceding-sibling::*) = 0">
		  <xsl:text>&cr;\gtst</xsl:text>
		</xsl:when>
		<xsl:otherwise>
		  <xsl:text>&cr;\tst</xsl:text>
		</xsl:otherwise>
	  </xsl:choose>
	</xsl:variable>
	-->
	<xsl:if test="preceding::Languages/@UseTestQuestionVernacular = 'true'
			and TestQuestionLine[@lang='Vernacular'][text()]
			and preceding::Languages/LanguageInfo[@lang='Vernacular'][@code]">
	  <xsl:text>&cr;\tst</xsl:text>
	  <xsl:value-of select="preceding::Languages/LanguageInfo[@lang='Vernacular']/@code"/>
	  <xsl:text> </xsl:text>
	  <xsl:value-of select="TestQuestionLine[@lang='Vernacular']"/>
	</xsl:if>
	<xsl:if test="preceding::Languages/@UseTestQuestionNationalBT = 'true'
			and TestQuestionLine[@lang='NationalBt'][text()]
			and preceding::Languages/LanguageInfo[@lang='NationalBt'][@code]">
	  <xsl:text>&cr;\tst</xsl:text>
	  <xsl:value-of select="preceding::Languages/LanguageInfo[@lang='NationalBt']/@code"/>
	  <xsl:text> </xsl:text>
	  <xsl:value-of select="TestQuestionLine[@lang='NationalBt']"/>
	</xsl:if>
	<xsl:if test="preceding::Languages/@UseTestQuestionInternationalBT = 'true'
			and TestQuestionLine[@lang='InternationalBt'][text()]
			and preceding::Languages/LanguageInfo[@lang='InternationalBt'][@code]">
	  <xsl:text>&cr;\tst</xsl:text>
	  <xsl:value-of select="preceding::Languages/LanguageInfo[@lang='InternationalBt']/@code"/>
	  <xsl:text> </xsl:text>
	  <xsl:value-of select="TestQuestionLine[@lang='InternationalBt']"/>
	</xsl:if>
	<xsl:apply-templates select="Answers"/>
  </xsl:template>

  <xsl:template match="Answers">
	  <xsl:apply-templates/>
  </xsl:template>

  <xsl:template match="Answer">
	<xsl:if test="preceding::Languages/@UseAnswerVernacular = 'true'
			and @lang = 'Vernacular'
			and preceding::Languages/LanguageInfo[@lang='Vernacular'][@code]">
	  <xsl:text>&cr;\ans</xsl:text>
	  <xsl:value-of select="preceding::Languages/LanguageInfo[@lang='Vernacular']/@code"/>
	  <xsl:text> T</xsl:text>
	  <xsl:number value="position()" format="1"/>
	  <xsl:text>: </xsl:text>
	  <xsl:value-of select="."/>
	</xsl:if>
	<xsl:if test="preceding::Languages/@UseAnswerNationalBT = 'true'
			and @lang = 'NationalBt'
			and preceding::Languages/LanguageInfo[@lang='NationalBt'][@code]">
	  <xsl:text>&cr;\ans</xsl:text>
	  <xsl:value-of select="preceding::Languages/LanguageInfo[@lang='NationalBt']/@code"/>
	  <xsl:text> T</xsl:text>
	  <xsl:number value="position()" format="1"/>
	  <xsl:text>: </xsl:text>
	  <xsl:value-of select="."/>
	</xsl:if>
	<xsl:if test="preceding::Languages/@UseAnswerInternationalBT = 'true'
			and @lang = 'InternationalBt'
			and preceding::Languages/LanguageInfo[@lang='InternationalBt'][@code]">
	  <xsl:text>&cr;\ans</xsl:text>
	  <xsl:value-of select="preceding::Languages/LanguageInfo[@lang='InternationalBt']/@code"/>
	  <xsl:text> T</xsl:text>
	  <xsl:number value="position()" format="1"/>
	  <xsl:text>: </xsl:text>
	  <xsl:value-of select="."/>
	</xsl:if>
  </xsl:template>

  <!-- toss out anything else -->
  <xsl:template match="*"/>

</xsl:transform>
