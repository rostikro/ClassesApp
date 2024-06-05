import axios from "axios"
import { useEffect, useState } from "react";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faUserGraduate } from '@fortawesome/free-solid-svg-icons'
import Fab from '@mui/material/Fab';
import AddIcon from '@mui/icons-material/Add'
import { Modal } from "react-bootstrap";
import { useNavigate } from "react-router-dom";

function Classes() {
    const [classes, setClasses] = useState([]);

    const [classId, setClassId] = useState('');
    const [passcode, setPasscode] = useState('');

    const [name, setName] = useState('');
    const [subject, setSubject] = useState('');
    const [newClassPasscode, setNewClassPasscode] = useState('');

    const [enrollShow, setEnrollShow] = useState(false);
    const [createClassShow, setCreateClassShow] = useState(false);

    let navigate = useNavigate();

    useEffect(() => {
        axios.defaults.headers.common["Authorization"] = 'Bearer ' + localStorage.getItem("token");
        fetchAll();
    }, []);

    const handleShowEnrollModal = () => setEnrollShow(true);
    const handleCloseEnrollModal = () => setEnrollShow(false);

    const handleShowCreateClassModal = () => setCreateClassShow(true);
    const handleCloseCreateClassModal = () => setCreateClassShow(false);

    const fetchAll = async () => {
        axios.get('https://localhost:7066/api/Courses/getAll').then(response => {
            // handle success
            setClasses(response.data);
          });
    }

    const enroll = async() => {
        await axios({
            method: 'post',
            url: 'https://localhost:7066/api/Courses/enroll',
            data: {
              id: classId,
              passcode: passcode,
            }
          });

        setClassId(0);
        setPasscode('');
        fetchAll();
        handleCloseEnrollModal();
    }

    const createClass = async () => {
        await axios({
            method: 'post',
            url: 'https://localhost:7066/api/Courses/create',
            data: {
              name: name,
              subject: subject,
              passcode: newClassPasscode,
            }
          });

        setName('');
        setSubject('');
        setNewClassPasscode('');
        fetchAll();
        handleCloseCreateClassModal();
    }

    return (
        <div>
            <div className="floating-btns">
            <Fab variant="extended" size="small" color="primary" onClick={handleShowEnrollModal}>
                <AddIcon />
                Enroll
            </Fab>
            <Fab variant="extended" size="small" color="primary" onClick={handleShowCreateClassModal}>
                <AddIcon />
                Create New
            </Fab>
            </div>
        <div className="cards-container">
            {classes.map(c => (
                <div className="card-wrap">
                    <div className="card-h">
                    <FontAwesomeIcon icon={faUserGraduate} />
                    </div>
                    <div className="card-content">
                        <h1 className="card-title">{c.name}</h1>
                        <p className="card-text">Subject: {c.subject}</p>
                        <p className="card-text">Teacher: {c.teacher}</p>
                        <button className="card-btn" onClick={() => navigate('/class/'+c.id)}>Go →</button>
                    </div>
                </div>
            ))}
        </div>

        <Modal show={enrollShow} onHide={handleCloseEnrollModal}>
            <Modal.Header closeButton>
                <Modal.Title>Enroll the class</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <form onSubmit={(e) => {
                    e.preventDefault();
                    enroll();
                }}>
                    <div className="form-group mt-3">
                        <label>Class Id</label>
                        <input 
                        value={classId}
                        onChange={(e) => setClassId(e.target.value)}
                        className="form-control mt-1"
                        placeholder="Class Id"/>
                    </div>
                    <div className="form-group mt-3">
                        <label>Passcode</label>
                        <input 
                        value={passcode}
                        onChange={(e) => setPasscode(e.target.value)}
                        className="form-control mt-1"
                        placeholder="Passcode"/>
                    </div>
                    <div className="d-grid gap-2 mt-3">
                <button type="submit" className="btn btn-primary">
                  Enroll
                </button>
              </div>
                </form>
            </Modal.Body>
        </Modal>

        <Modal show={createClassShow} onHide={handleCloseCreateClassModal}>
            <Modal.Header closeButton>
                <Modal.Title>Create new class</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <form onSubmit={(e) => {
                    e.preventDefault();
                    createClass();
                }}>
                    <div className="form-group mt-3">
                        <label>Name</label>
                        <input 
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                        className="form-control mt-1"
                        placeholder="Name"/>
                    </div>
                    <div className="form-group mt-3">
                        <label>Subject</label>
                        <input 
                        value={subject}
                        onChange={(e) => setSubject(e.target.value)}
                        className="form-control mt-1"
                        placeholder="Subject"/>
                    </div>
                    <div className="form-group mt-3">
                        <label>Passcode</label>
                        <input 
                        value={passcode}
                        onChange={(e) => setNewClassPasscode(e.target.value)}
                        className="form-control mt-1"
                        placeholder="Passcode"/>
                    </div>
                    <div className="d-grid gap-2 mt-3">
                <button type="submit" className="btn btn-primary">
                  Create
                </button>
              </div>
                </form>
            </Modal.Body>
        </Modal>
        </div>
    );
}

export default Classes;