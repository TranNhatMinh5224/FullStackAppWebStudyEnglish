import React, { useState, useEffect } from "react";
import { Modal, Button, Form, Row, Col } from "react-bootstrap";
import { teacherPackageService } from "../../../Services/teacherPackageService";
import { toast } from "react-toastify";
import "./PackageFormModal.css";

export default function PackageFormModal({ show, onClose, onSuccess, packageToEdit }) {
    const [formData, setFormData] = useState({
        packageName: "",
        level: 1,  // Basic = 1 in backend enum
        price: "",
        maxCourses: "",
        maxLessons: "",
        maxStudents: ""
    });
    const [loading, setLoading] = useState(false);
    const [errors, setErrors] = useState({
        price: "",
        maxCourses: "",
        maxLessons: "",
        maxStudents: ""
    });

    useEffect(() => {
        if (packageToEdit) {
            setFormData({
                packageName: packageToEdit.packageName || packageToEdit.PackageName || "",
                level: packageToEdit.level || packageToEdit.Level || 1,
                price: (packageToEdit.price || packageToEdit.Price || 0).toString(),
                maxCourses: (packageToEdit.maxCourses || packageToEdit.MaxCourses || 5).toString(),
                maxLessons: (packageToEdit.maxLessons || packageToEdit.MaxLessons || 50).toString(),
                maxStudents: (packageToEdit.maxStudents || packageToEdit.MaxStudents || 100).toString()
            });
        } else {
            setFormData({
                packageName: "",
                level: 1,
                price: "",
                maxCourses: "",
                maxLessons: "",
                maxStudents: ""
            });
        }
        setErrors({
            price: "",
            maxCourses: "",
            maxLessons: "",
            maxStudents: ""
        });
    }, [packageToEdit, show]);

    const handleChange = (e) => {
        const { name, value } = e.target;
        
        // Đơn giản: chỉ lưu giá trị như các form khác
        setFormData({
            ...formData,
            [name]: value
        });
        
        // Clear error khi user đang gõ
        if (errors[name]) {
            setErrors({
                ...errors,
                [name]: ""
            });
        }
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        
        // Validate tên gói
        if (!formData.packageName.trim()) {
            toast.error("Vui lòng nhập tên gói");
            return;
        }
        
        // Validate và parse giá
        const price = formData.price === "" ? 0 : parseInt(formData.price.toString().replace(/[^\d]/g, ''), 10);
        if (isNaN(price) || price < 0) {
            setErrors({ ...errors, price: "Giá phải là số hợp lệ và không được âm" });
            return;
        }
        if (price > 100000000) {
            setErrors({ ...errors, price: "Giá không được vượt quá 100,000,000 VND" });
            return;
        }
        
        // Validate và parse các trường tài nguyên
        const maxCourses = formData.maxCourses === "" ? 0 : parseInt(formData.maxCourses, 10);
        if (isNaN(maxCourses) || maxCourses < 1 || maxCourses > 100) {
            setErrors({ ...errors, maxCourses: "Max khóa học phải từ 1 đến 100" });
            toast.error("Vui lòng kiểm tra lại các giá trị tài nguyên");
            return;
        }
        
        const maxLessons = formData.maxLessons === "" ? 0 : parseInt(formData.maxLessons, 10);
        if (isNaN(maxLessons) || maxLessons < 1 || maxLessons > 1000) {
            setErrors({ ...errors, maxLessons: "Max bài học phải từ 1 đến 1,000" });
            toast.error("Vui lòng kiểm tra lại các giá trị tài nguyên");
            return;
        }
        
        const maxStudents = formData.maxStudents === "" ? 0 : parseInt(formData.maxStudents, 10);
        if (isNaN(maxStudents) || maxStudents < 1 || maxStudents > 10000) {
            setErrors({ ...errors, maxStudents: "Max học viên phải từ 1 đến 10,000" });
            toast.error("Vui lòng kiểm tra lại các giá trị tài nguyên");
            return;
        }
        
        setLoading(true);
        setErrors({
            price: "",
            maxCourses: "",
            maxLessons: "",
            maxStudents: ""
        });
        
        try {
            // Prepare data matching backend DTO exactly
            const dataToSend = {
                packageName: formData.packageName.trim(),
                level: Number(formData.level),
                price: price,
                maxCourses: maxCourses,
                maxLessons: maxLessons,
                maxStudents: maxStudents
            };

            console.log("Data being sent to API:", dataToSend);

            let response;
            if (packageToEdit) {
                const id = packageToEdit.teacherPackageId || packageToEdit.TeacherPackageId;
                response = await teacherPackageService.update(id, dataToSend);
            } else {
                response = await teacherPackageService.create(dataToSend);
            }

            console.log("API Response:", response);

            if (response.data?.success) {
                const message = packageToEdit ? "Cập nhật gói thành công!" : "Tạo gói mới thành công!";
                onClose();
                onSuccess(message);
            } else {
                toast.error(response.data?.message || "Có lỗi xảy ra.");
            }
        } catch (error) {
            console.error("Error saving package:", error);
            console.error("Error details:", error.response?.data);
            toast.error(error.response?.data?.message || error.response?.data?.title || "Lỗi kết nối.");
        } finally {
            setLoading(false);
        }
    };

    return (
        <Modal 
            show={show} 
            onHide={onClose} 
            centered 
            className="modal-modern package-form-modal"
            dialogClassName="package-form-modal-dialog"
        >
            <Modal.Header closeButton>
                <Modal.Title>{packageToEdit ? "Cập nhật Gói" : "Tạo Teacher Package"}</Modal.Title>
            </Modal.Header>
            <Form onSubmit={handleSubmit}>
                <Modal.Body>
                    <Row className="mb-3">
                        <Col md={6}>
                            <Form.Group>
                                <Form.Label>Tên gói <span className="text-danger">*</span></Form.Label>
                                <Form.Control
                                    type="text"
                                    name="packageName"
                                    value={formData.packageName}
                                    onChange={handleChange}
                                    required
                                    placeholder="VD: Basic Plan"
                                />
                            </Form.Group>
                        </Col>
                        <Col md={6}>
                            <Form.Group>
                                <Form.Label>Cấp độ (Level)</Form.Label>
                                <Form.Select
                                    name="level"
                                    value={formData.level}
                                    onChange={handleChange}
                                >
                                    <option value={1}>Basic</option>
                                    <option value={2}>Standard</option>
                                    <option value={3}>Premium</option>
                                    <option value={4}>Professional</option>
                                </Form.Select>
                            </Form.Group>
                        </Col>
                    </Row>
                    <Row className="mb-3">
                        <Col md={12}>
                            <Form.Group>
                                <Form.Label>Giá (VND) <span className="text-danger">*</span></Form.Label>
                                <Form.Control
                                    type="number"
                                    name="price"
                                    value={formData.price}
                                    onChange={handleChange}
                                    placeholder="VD: 1000000"
                                    required
                                    min="0"
                                    max="100000000"
                                    isInvalid={!!errors.price}
                                />
                                {errors.price && (
                                    <Form.Control.Feedback type="invalid">
                                        {errors.price}
                                    </Form.Control.Feedback>
                                )}
                                <Form.Text className="text-muted">
                                    Nhập giá từ 0 đến 100,000,000 VND
                                </Form.Text>
                            </Form.Group>
                        </Col>
                    </Row>
                    <h6 className="mt-4 mb-3 text-primary border-bottom pb-2">Giới hạn tài nguyên</h6>
                    <Row className="mb-3">
                        <Col md={4}>
                            <Form.Group>
                                <Form.Label>Max Khóa học</Form.Label>
                                <Form.Control
                                    type="number"
                                    name="maxCourses"
                                    value={formData.maxCourses}
                                    onChange={handleChange}
                                    min="1"
                                    max="100"
                                    required
                                    isInvalid={!!errors.maxCourses}
                                />
                                {errors.maxCourses && (
                                    <Form.Control.Feedback type="invalid">
                                        {errors.maxCourses}
                                    </Form.Control.Feedback>
                                )}
                                <Form.Text className="text-muted">
                                    Từ 1 đến 100
                                </Form.Text>
                            </Form.Group>
                        </Col>
                        <Col md={4}>
                            <Form.Group>
                                <Form.Label>Max Bài học</Form.Label>
                                <Form.Control
                                    type="number"
                                    name="maxLessons"
                                    value={formData.maxLessons}
                                    onChange={handleChange}
                                    min="1"
                                    max="1000"
                                    required
                                    isInvalid={!!errors.maxLessons}
                                />
                                {errors.maxLessons && (
                                    <Form.Control.Feedback type="invalid">
                                        {errors.maxLessons}
                                    </Form.Control.Feedback>
                                )}
                                <Form.Text className="text-muted">
                                    Từ 1 đến 1,000
                                </Form.Text>
                            </Form.Group>
                        </Col>
                        <Col md={4}>
                            <Form.Group>
                                <Form.Label>Max Học viên</Form.Label>
                                <Form.Control
                                    type="number"
                                    name="maxStudents"
                                    value={formData.maxStudents}
                                    onChange={handleChange}
                                    min="1"
                                    max="10000"
                                    required
                                    isInvalid={!!errors.maxStudents}
                                />
                                {errors.maxStudents && (
                                    <Form.Control.Feedback type="invalid">
                                        {errors.maxStudents}
                                    </Form.Control.Feedback>
                                )}
                                <Form.Text className="text-muted">
                                    Từ 1 đến 10,000
                                </Form.Text>
                            </Form.Group>
                        </Col>
                    </Row>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={onClose} disabled={loading}>
                        Hủy
                    </Button>
                    <Button variant="primary" type="submit" disabled={loading}>
                        {loading ? "Đang lưu..." : "Lưu"}
                    </Button>
                </Modal.Footer>
            </Form>
        </Modal>
    );
}
