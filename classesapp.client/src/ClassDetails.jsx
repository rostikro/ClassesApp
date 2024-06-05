import { useEffect, useState } from "react";
import axios from "axios"
import { useParams } from "react-router-dom";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faClipboardList } from '@fortawesome/free-solid-svg-icons'
import { useNavigate } from "react-router-dom";

function ClassDetails() {
    const {id} = useParams();
    let navigate = useNavigate();

    const [classDetails, setClassDetails] = useState(null);
    useEffect(() => {
        axios.defaults.headers.common["Authorization"] = 'Bearer ' + localStorage.getItem("token");
        fetchClassDetails();
    }, []);

    const fetchClassDetails = async () => {
        console.log(id);
        axios.get('https://localhost:7066/api/Courses/get/'+id).then(response => {
            // handle success
            setClassDetails(response.data);
          });
    }

    return (
        <div className="container">
            <div className="class-header">
                <div className="text-group">
                    <h1 className="class-name">{classDetails?.name}</h1>
                    <p className="class-text">Subject: {classDetails?.subject}</p>
                    <p className="class-text">Teacher: {classDetails?.teacher}</p>
                </div>
            </div>
            <div className="tasks-container">
                <ul className="task-list">
                {classDetails?.tasks.map(t => (
                    <li className="task" onClick={() => navigate('/task/'+t.id)}>
                        <FontAwesomeIcon className="clipboard" icon={faClipboardList} />
                        <p className="task-title">{classDetails?.teacher} posted a new task: {t.title}</p>
                    </li>
                    ))}
                </ul>
            </div>
        </div>
    );
}

export default ClassDetails;