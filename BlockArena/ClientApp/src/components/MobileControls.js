import { keys } from '../core/constants'
import React from 'react'
import styled from 'styled-components';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faArrowLeft, faArrowRight, faArrowDown, faPause, faRotate } from '@fortawesome/free-solid-svg-icons';

const LeftButton = styled.button`
    position: fixed;
    left: 0px;
    top: 0px;
    width: 15%;
    height: 100%;
    opacity: 0.5;
`;

//Rotate
const TopButton = styled.button`
    position: fixed;
    left: 18%;
    top: 17px;
    width: 30%;
    height: 6%
    opacity: 0.5;
`;

//pause
const TopStackedButton = styled.button`
    position: fixed;
    left: 50%;
    top: 2%;
    width: 26%;
    height: 6%;
    opacity: 0.7;
`;

const RighButton = styled.button`
    position: fixed;
    top: 0px;
    right: 0px;
    width: 15%;
    height: 100%;
    opacity: 0.5;
`;

const BottomStackedButton = styled.button`
    position: fixed;
    left: 15%;
    bottom: 21%;
    width: 70%;
    height: 5%;
    opacity: 0.5;
`;

const BottomButton = styled.button`
    position: fixed;
    left: 15%;
    bottom: 35px;
    width: 70%;
    height: 5%;
    opacity: 0.5;
`;

export function MobileControls(props) {
    return <div style={{ position: "fixed", zIndex: 1 }}>
        <LeftButton className="btn btn-primary" onClick={() => props.onClick && props.onClick(keys.left)}>
            <FontAwesomeIcon icon={faArrowLeft} />
        </LeftButton>
        <RighButton className="btn btn-primary" onClick={() => props.onClick && props.onClick(keys.right)}>
            <FontAwesomeIcon icon={faArrowRight} />
        </RighButton>
        <TopButton className="btn btn-primary" onClick={() => props.onClick && props.onClick(keys.up)}>
            <FontAwesomeIcon icon={faRotate} />
            &nbsp;
            <b>Rotate</b>
        </TopButton>
        {props.onPause && <TopStackedButton className="btn btn-primary" onClick={props.onPause}>
            <FontAwesomeIcon icon={faPause} />
            &nbsp;
            <b>Pause</b>
        </TopStackedButton>}
        <BottomStackedButton className="btn btn-primary" onClick={() => props.onClick && props.onClick(keys.down)}>
            <FontAwesomeIcon icon={faArrowDown} />
        </BottomStackedButton>
        <BottomButton className="btn btn-primary" onClick={() => props.onClick && props.onClick(keys.space)}>
            <b>Commit</b>
        </BottomButton>
    </div>;
}