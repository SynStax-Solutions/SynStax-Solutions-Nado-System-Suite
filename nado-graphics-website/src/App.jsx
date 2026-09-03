import { BrowserRouter as Router, Routes, Route } from 'react-router-dom'

import Navbar from './components/layout/Navbar'
import Footer from './components/layout/Footer'

import Hero from './components/sections/Hero'
import Marquee from './components/sections/Marquee'
import About from './components/sections/About'
import WorkGrid from './components/sections/WorkGrid'
import Process from './components/sections/Process'
import CTA from './components/sections/CTA'

import SmoothScroll from './SmoothScroll'

import AboutPage from './pages/AboutPage'
import PortfolioPage from './pages/PortfolioPage'
import ContactPage from './pages/ContactPage'

import './App.css'


function HomePage() {
  return (
    <div className="app">
      <Hero />
      <Marquee />
      <About />
      <WorkGrid />
      <Process />
      <CTA />
    </div>
  )
}


export default function App() {
  return (
    <Router>
      <SmoothScroll />

      <div className="app">
        <Navbar />

        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/about" element={<AboutPage />} />
          <Route path="/portfolio" element={<PortfolioPage />} />
          <Route path="/contact" element={<ContactPage />} />
        </Routes>

        <Footer />
      </div>
    </Router>
  )
}