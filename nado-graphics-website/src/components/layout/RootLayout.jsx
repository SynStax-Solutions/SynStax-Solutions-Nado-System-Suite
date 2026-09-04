import React, { useEffect } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import gsap from 'gsap';
import { ScrollTrigger } from 'gsap/ScrollTrigger';
import Lenis from 'lenis';

import Footer from './Footer';

gsap.registerPlugin(ScrollTrigger);

export default function RootLayout() {
  const { pathname } = useLocation();

  useEffect(() => {
    // Instantiate Lenis smooth scroller
    const lenis = new Lenis({
      lerp: 0.07,
      duration: 1.2,
      smoothWheel: true,
      wheelMultiplier: 1.0,
      touchMultiplier: 1.5,
      autoRaf: false, 
    });

    // Notify ScrollTrigger on Lenis scroll
    lenis.on('scroll', ScrollTrigger.update);

    // 3. Advance Lenis on every GSAP frame
    const updateGSAP = (time) => {
      lenis.raf(time * 1000);
    };

    gsap.ticker.add(updateGSAP);
    gsap.ticker.lagSmoothing(0);

    return () => {
      gsap.ticker.remove(updateGSAP);
      lenis.destroy();
    };
  }, []);

  // Reset scroll position to top on page navigation
  useEffect(() => {
    window.scrollTo(0, 0);
    ScrollTrigger.refresh();
  }, [pathname]);

  return (
    <div id="smooth-wrapper">
      <div id="smooth-content">
        <main>
          <Outlet />
        </main>
        <Footer />
      </div>
    </div>
  );
}