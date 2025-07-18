import React from 'react';
import { useLocalPlayerGameContext } from './LocalPlayerGame';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faArrowLeft, faArrowRight, faArrowDown, faRotate, faArrowUp } from '@fortawesome/free-solid-svg-icons';
import { MovingDown, MovingLeft, MovingRight, RotatingIcon } from './components/AnimatedIcons';

export const ControlLegend = () => {
    const { game } = useLocalPlayerGameContext();
    const isMobile = game?.mobile;

    return !isMobile && <div className="card mt-3 mb-3">
        <div className="card-body">
            <h5 className="card-title">Управление</h5>
            <div className="card-text">
                <table className="table">
                    <thead>
                        <tr>
                            <th>Ключ</th>
                            <th>Команда</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>
                                <FontAwesomeIcon icon={faArrowUp} />
                            </td>
                            <td>
                                Поворот <RotatingIcon icon={faRotate} />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <FontAwesomeIcon icon={faArrowDown} />
                            </td>
                            <td>
                                Сдвиг <MovingDown icon={faArrowDown} />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <FontAwesomeIcon icon={faArrowLeft} />
                            </td>
                            <td>
                                Сдвиг <MovingLeft icon={faArrowLeft} />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <FontAwesomeIcon icon={faArrowRight} />
                            </td>
                            <td>
                                Сдвиг <MovingRight icon={faArrowRight} />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                [Space]
                            </td>
                            <td>
                                Мгновенное падение фигуры
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>;
}