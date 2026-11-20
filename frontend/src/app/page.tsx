'use client';
import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { auth } from '@/lib/auth';

export default function Home() {
  const router = useRouter();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
    router.replace(auth.isAuthenticated() ? '/dashboard' : '/login');
  }, [router]);

  if (!mounted) return null;
  return null;
}
