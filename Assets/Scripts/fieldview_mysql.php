 <?php
     $hostname = 'studies.cu-visualab.org';
     $username = 'visualab';
     $database = 'fieldview';
 
     if (isset($_REQUEST["password"])) {
         $password = $_REQUEST["password"];
     }
     $conn = new mysqli($hostname, $username, $password, $database);
 
     if($conn->connect_error){
 
         echo "Failed to connect " . $conn->connect_error;
     }
 
     $result = $conn->query("Select * FROM Fourteeners");
     $counter = 0;
     echo "[";
     if ($result->num_rows > 0) {
         // output data of each row
         while($row = $result->fetch_assoc()) {
             echo "{\"peak\": \"" . $row["Peak"]. "\",\"longitude\": \"" . $row["Longitude"]. "\",\"latitude\": \"" . $row["Latitude"] . "\",\"elevation\": \"" . $row["Elevation"] . "\"}";
             if (++$counter == $result->num_rows) {
                 
             } else {
                 echo ",";
             }
         }
     }
     echo "]";
     $conn->close();
 ?>