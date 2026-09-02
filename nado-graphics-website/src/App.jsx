import Navbar from './components/layout/Navbar'
import Footer from './components/layout/Footer'
import Hero from './components/sections/Hero'
import Marquee from './components/sections/Marquee'
import About from './components/sections/About'
import WorkGrid from './components/sections/WorkGrid'
import Process from './components/sections/Process'
import CTA from './components/sections/CTA'
import './App.css'

function App() {
  return (
    <div className="app">
      <Navbar />
      <Hero />
      <Marquee />
      <About />
      <WorkGrid />
      <Process />
      <CTA />
      <Footer />
    </div>
  )
}

export default App