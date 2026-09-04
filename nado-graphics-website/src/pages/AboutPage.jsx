// src/pages/AboutPage.jsx
import React from 'react';
import AboutHero from '../components/sections/AboutHero';
import Story from '../components/sections/Story';
import Values from '../components/sections/Values';
import Approach from '../components/sections/Approach';
import Team from '../components/sections/Team';
import './AboutPage.css';

export default function AboutPage() {
  return (
    <div className="about-page">
      <AboutHero />
      <Story />
      <Values />
      <Approach />
      <Team />
    </div>
  );
}
