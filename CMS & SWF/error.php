<?php

require_once('./data_classes/server-data.php_data_classes-core.php.php');
require_once('./data_classes/server-data.php_data_classes-session.php.php');

require_once('./templates/community_subheader.php');
require_once('./templates/community_header.php');

?>

<div id="container">
<div id="content" style="position: relative" class="clearfix">
<div id="column1" class="column">
<div class="habblet-container ">
<div class="cbb clearfix red ">
<h2 class="title">Bobba! Algo deu errado..</h2>
<div id="notfound-content" class="box-content">
<img id="error-image" src="http://i.imgur.com/Maq3itv.gif"/>
<br><br> <p class="error-text">N�o � poss�vel encontrar a p�gina que voc� est� procurando. Por favor verifique a URL ou tente come�ar de novo a partir da <a href="/me">P�gina Principal</a>.</p>
</div></div>
</div>
<script type="text/javascript">if (!$(document.body).hasClassName('process-template')) { Rounder.init(); }</script>
</div>
<script type="text/javascript">
			HabboView.run();
		</script>
<?php require_once('./templates/community_footer.php'); ?>
</div>
</div>
</div>
</body>