                                    <ul id="feed-items">

									
<?php
$sql = mysql_query("SELECT * FROM messenger_requests WHERE from_id = '".$my_id."'");
$count = mysql_num_rows($sql);
if($count != 0){
?>
			<li id="feed-notification">
			Voc� tem <font color="#FFFFFF"><?php echo $count; ?> pedidos de amizade</font> pendentes.<br/>Entre no Hotel para aceitar ou recusa-los.
			</li>
			
<?php } ?>
									
									<li class="contributed" style="background-image: url('/web-gallery/v2/images/fb.png') !important; padding-left: 65px; padding-bottom: 15px;">
<b>Curta nosso Facebook</b><br>
Oi <?php echo $name; ?>, quer saber de tudo o que rola dentro e fora do hotel? Ent�o curta nossa <a href="https://www.facebook.com/*" target="_blank">p�gina no facebook</a>!
</li> 

									
									<li class="small" id="feed-lastlogin"> 
                Seu �ltimo acesso foi:
                <?php echo date('d/m/Y H:i:s', $myrow['last_online']); echo""; ?>
               </li>
                                    </ul>